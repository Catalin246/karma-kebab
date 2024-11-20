package main

import (
	"event-service/db"
	"event-service/routes"
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

	// Initialize Azure Table Storage
	db.InitAzureTables(connectionString)

	// Register routes
	router := routes.RegisterRoutes()

	// Start the server
	log.Println("Server is running on port 3001")
	log.Fatal(http.ListenAndServe(":3001", router))
}
