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
	accountName := os.Getenv("AZURE_ACCOUNT_NAME")
	accountKey := os.Getenv("AZURE_ACCOUNT_KEY")
	tableName := os.Getenv("AZURE_CONTAINER_NAME")

	// Initialize Azure Table Storage
	db.InitAzureTable(accountName, accountKey, tableName)

	// Register routes
	router := mux.NewRouter()

	// Start the server
	log.Println("Server is running on port 3001")
	log.Fatal(http.ListenAndServe(":3001", router))
}
