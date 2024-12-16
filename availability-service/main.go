package main

import (
	"availability-service/db"
	"availability-service/routes"
	"log"
	"net/http"
	"os"

	"github.com/joho/godotenv"
)

func main() {
	// Try loading the .env file (optional for production)
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

	// Register routes with the service client
	router := routes.RegisterRoutes(client)

	// Start the server
	log.Println("Server is running on port 3002")
	log.Fatal(http.ListenAndServe(":3002", router))
}
