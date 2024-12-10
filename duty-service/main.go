package main

import (
	"duty-service/db"
	"duty-service/routes"
	"log"
	"net/http"
	"os"

	"github.com/joho/godotenv"
)

func main() {
	// Load environment variables
	if err := godotenv.Load(".env"); err != nil {
		log.Fatal("Error loading .env file: ", err)
	}

	// Get environment variables
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
	log.Println("Server is running on port 3004")
	log.Fatal(http.ListenAndServe(":3004", router))
}
