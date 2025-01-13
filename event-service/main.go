package main

import (
	"encoding/base64"
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

// failOnError logs the error and exits the program if the error is not nil
func failOnError(err error, msg string) {
	if err != nil {
		log.Fatalf("%s: %s", msg, err)
	}
}

func main() {
	// Try loading the .env file (optional for production)
	if err := godotenv.Load(".env"); err != nil {
		log.Println("Warning: .env file not found, falling back to environment variables")
	}

	// Fetch the base64 encoded public key PEM from environment variables
	encodedPEM := os.Getenv("PUBLIC_KEY_PEM")
	if encodedPEM == "" {
		log.Fatal("Error: PUBLIC_KEY_PEM is not set in the environment")
	}

	// Decode the base64 string
	publicKeyPEM, err := base64.StdEncoding.DecodeString(encodedPEM)
	if err != nil {
		log.Fatalf("Error decoding base64 PEM: %v", err)
	}

	// Log the decoded PEM for verification
	log.Printf("Decoded PEM: %s", string(publicKeyPEM))

	// Fetch environment variable
	connectionString := os.Getenv("AZURE_STORAGE_CONNECTION_STRING")
	if connectionString == "" {
		log.Fatal("Error: AZURE_STORAGE_CONNECTION_STRING is not set")
	}

	// Initialize Azure Table Storage
	client, err := db.InitAzureTables(connectionString)
	if err != nil {
		log.Fatal("Error initializing Azure Table Storage: ", err)
	}

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
	rabbitMQService := services.NewRabbitMQService(ch, client)

	// Start consuming messages from the "shiftCreated"
	err = rabbitMQService.ConsumeMessage("shiftCreated")
	if err != nil {
		log.Fatalf("Error consuming messages: %v", err)
	}

	metrics.RegisterMetricsHandler()

	// Register routes with the service client and RabbitMQService
	router := routes.RegisterRoutes(client, rabbitMQService, string(publicKeyPEM))

	// Start the server
	log.Println("Server is running on port 3001")
	log.Fatal(http.ListenAndServe(":3001", router))
}
