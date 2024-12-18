package main

import (
	"duty-service/db"
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
		log.Fatal("Error loading .env file: ", err)
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

	// Register HTTP routes
	router := routes.RegisterRoutes(tableClient, blobServiceClient)

	// Start the server
	log.Println("Server is running on port 3004")
	log.Fatal(http.ListenAndServe(":3004", router))
}