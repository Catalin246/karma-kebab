package main

import (
	"log"
	"net/http"
	"os"

	"github.com/Catalin246/karma-kebab/db"
	"github.com/Catalin246/karma-kebab/metrics"
	"github.com/Catalin246/karma-kebab/routes"
	"github.com/Catalin246/karma-kebab/services"

	"github.com/joho/godotenv"
	amqp "github.com/rabbitmq/amqp091-go"
)

func failOnError(err error, msg string) {
	if err != nil {
		log.Fatalf("%s: %s", msg, err)
	}
}

func main() {
	// Load environment variables
	if err := godotenv.Load(".env"); err != nil {
		log.Println("Warning: .env file not found, falling back to environment variables")
	}

	connectionString := os.Getenv("AZURE_STORAGE_CONNECTION_STRING")
	if connectionString == "" {
		log.Fatal("Error: AZURE_STORAGE_CONNECTION_STRING is not set")
	}

	// Initialize Azure Table Storage
	client, err := db.InitAzureTables(connectionString)
	failOnError(err, "Error initializing Azure Table Storage")

	// Initialize RabbitMQ
	rabbitmqUrl := os.Getenv("RABBITMQ_URL")
	if rabbitmqUrl == "" {
		log.Fatal("Error: RABBITMQ_URL is not set")
	}
	conn, err := amqp.Dial(rabbitmqUrl)
	failOnError(err, "Failed to connect to RabbitMQ")
	defer conn.Close()

	ch, err := conn.Channel()
	failOnError(err, "Failed to open a channel")
	defer ch.Close()

	// Initialize RabbitMQService
	rabbitMQService := services.NewRabbitMQService(ch) // Pass both arguments
	failOnError(err, "Failed to initialize RabbitMQService")

	// Start consuming messages
	err = rabbitMQService.ConsumeMessage("shiftCreated")
	failOnError(err, "Error consuming messages")

	// Register routes
	metrics.RegisterMetricsHandler()
	router := routes.RegisterRoutes(client, rabbitMQService)

	// Start the server
	log.Println("Server is running on port 3001")
	log.Fatal(http.ListenAndServe(":3001", router))
}
