package main

import (
	"availability-service/repository"
	"availability-service/service"
	"context"
	"fmt"
	"log"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
	"github.com/gin-gonic/gin"
	"github.com/spf13/viper"
)

type Config struct {
	Server struct {
		Port int    `mapstructure:"port"`
		Host string `mapstructure:"host"`
	} `mapstructure:"server"`
	Cosmos struct {
		Endpoint string `mapstructure:"endpoint"`
		Key      string `mapstructure:"key"`
		Database string `mapstructure:"database"`
		Container string `mapstructure:"container"`
	} `mapstructure:"cosmos"`
}

func loadConfig() (*Config, error) {
	viper.SetConfigName("config")
	viper.SetConfigType("yaml")
	viper.AddConfigPath("./configs")
	viper.AddConfigPath(".")
	
	viper.AutomaticEnv()

	if err := viper.ReadInConfig(); err != nil {
		return nil, fmt.Errorf("failed to read config: %w", err)
	}

	var config Config
	if err := viper.Unmarshal(&config); err != nil {
		return nil, fmt.Errorf("failed to unmarshal config: %w", err)
	}

	return &config, nil
}

func setupCosmosClient(config *Config) (*azcosmos.Client, error) {
	cred, err := azcosmos.NewKeyCredential(config.Cosmos.Key)
	if err != nil {
		return nil, fmt.Errorf("failed to create cosmos credentials: %w", err)
	}

	client, err := azcosmos.NewClientWithKey(config.Cosmos.Endpoint, cred, nil)
	if err != nil {
		return nil, fmt.Errorf("failed to create cosmos client: %w", err)
	}

	return client, nil
}

func main() {
	// Load configuration
	config, err := loadConfig()
	if err != nil {
		log.Fatalf("Failed to load configuration: %v", err)
	}

	// Setup Cosmos DB client
	cosmosClient, err := setupCosmosClient(config)
	if err != nil {
		log.Fatalf("Failed to setup Cosmos DB client: %v", err)
	}

	// Initialize repository
	container, err := cosmosClient.NewContainer(config.Cosmos.Database, config.Cosmos.Container)
	if err != nil {
		log.Fatalf("Failed to get container reference: %v", err)
	}
	
	repo := repository.NewCosmosAvailabilityRepository(container)

	// Initialize service
	availabilityService := service.NewAvailabilityService(repo)

	// Setup Gin router
	router := gin.Default()
	
	// Add middleware
	router.Use(gin.Recovery())
	router.Use(gin.Logger())

	// Initialize handlers
	handlers.NewAvailabilityHandler(router, availabilityService)

	// Create HTTP server
	srv := &http.Server{
		Addr:    fmt.Sprintf("%s:%d", config.Server.Host, config.Server.Port),
		Handler: router,
	}

	// Start server in a goroutine
	go func() {
		log.Printf("Starting server on %s:%d", config.Server.Host, config.Server.Port)
		if err := srv.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			log.Fatalf("Failed to start server: %v", err)
		}
	}()

	// Setup graceful shutdown
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit
	log.Println("Shutting down server...")

	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	if err := srv.Shutdown(ctx); err != nil {
		log.Fatalf("Server forced to shutdown: %v", err)
	}

	log.Println("Server exiting")
}