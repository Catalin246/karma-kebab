package main

import (
	"duty-service/db"
	"duty-service/metrics"
	"duty-service/routes"
	"duty-service/services"
	"log"
	"net/http"
	"os"

	"github.com/joho/godotenv"
	amqp "github.com/rabbitmq/amqp091-go"
)

func failOnError(err error, msg string) {
	if err != nil {
		log.Panicf("%s: %s", msg, err)
	}
}

func main() {
	// Load environment variables
	if err := godotenv.Load(".env"); err != nil {
		log.Println("Warning: .env file not found, falling back to environment variables")
	}

	// Get Azure Table Storage connection string
	tableConnectionString := os.Getenv("AZURE_STORAGE_CONNECTION_STRING")
	if tableConnectionString == "" {
		log.Fatal("Error: AZURE_STORAGE_CONNECTION_STRING is not set")
	}

	// Get Azure Blob Storage connection string
	blobConnectionString := os.Getenv("AZURE_STORAGE_CONNECTION_STRING")
	if blobConnectionString == "" {
		log.Fatal("Error: AZURE_STORAGE_CONNECTION_STRING for blob is not set")
	}

	// Fetch the public key PEM from environment variables
	publicKeyPEM := os.Getenv("PUBLIC_KEY_PEM")
	if publicKeyPEM == "" {
		log.Fatal("Error: PUBLIC_KEY_PEM is not set in the environment")
	}

	// Initialize Azure Table Storage
	tableClient, err := db.InitAzureTables(tableConnectionString)
	if err != nil {
		log.Fatal("Error initializing Azure Table Storage: ", err)
	}

	// Initialize Azure Blob Storage
	blobServiceClient, err := db.InitAzureBlobStorage(blobConnectionString)
	if err != nil {
		log.Fatal("Error initializing Azure Blob Storage: ", err)
	}

	// Initialize RabbitMQ connection
	rabbitmqURL := os.Getenv("RABBITMQ_URL")
	if rabbitmqURL == "" {
		log.Fatal("Error: RABBITMQ_URL is not set")
	}

	rabbitConn, err := amqp.Dial(rabbitmqURL)
	failOnError(err, "Failed to connect to RabbitMQ")
	defer rabbitConn.Close()

	// Initialize DutyAssignmentService
	dutyService := &services.DutyAssignmentService{}

	// Initialize RabbitMQService
	rabbitMQService := services.NewRabbitMQService(dutyService, rabbitConn)
	defer rabbitMQService.Close()

	// Register the /metrics route for Prometheus to scrape
	metrics.RegisterMetricsHandler()

	// Register HTTP routes
	router := routes.RegisterRoutes(tableClient, blobServiceClient, publicKeyPEM)

	// Fixed port: 3004
	port := "3004"

	// Start the server on port 3004
	log.Println("Server is running on port", port)
	log.Fatal(http.ListenAndServe(":"+port, router))

}
