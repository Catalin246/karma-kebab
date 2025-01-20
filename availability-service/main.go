package main

import (
	"availability-service/db"
	"availability-service/routes"
	"availability-service/service"
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
	// Try loading the .env file
	if err := godotenv.Load(".env"); err != nil {
		log.Println("Warning: .env file not found, falling back to environment variables")
	}

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
	conn, err := amqp.Dial("amqp://guest:guest@rabbitmq:5672/")
	failOnError(err, "Failed to connect to RabbitMQ")
	defer conn.Close()

	ch, err := conn.Channel()
	failOnError(err, "Failed to open a channel")
	defer ch.Close()

	// Initialize RabbitMQService 
	rabbitMQService := service.NewRabbitMQService(ch)

	// Register routes with the service client and RabbitMQService
	router := routes.RegisterRoutes(client, rabbitMQService)

	// Start the server
	log.Println("Server is running on port 3002")
	log.Fatal(http.ListenAndServe(":3002", router))
}