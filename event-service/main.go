package main

import (
	"event-service/db"
	"log"
	"net/http"
	"os"

	"github.com/gorilla/mux"
	"github.com/joho/godotenv"
)

func main() {
	// Load enviorment variables
	if err := godotenv.Load(".env"); err != nil {
		log.Fatal("Erros loading .env file: ", err)
	}

	// Get enviorment variables
	connectionString := os.Getenv("AZURE_STORAGE_CONNECTION_STRING")
	tableName := os.Getenv("AZURE_CONTAINER_NAME")

	// Initialize Azure Table Storage
	db.InitAzureTableWithConnectionString(connectionString, tableName)

	// Register routes
	router := mux.NewRouter()

	// Start the server
	log.Println("Server is running on port 3001")
	log.Fatal(http.ListenAndServe(":3001", router))
}
