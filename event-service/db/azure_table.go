package db

import (
	"context"
	"errors"
	"log"
	"net/http"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
)

// Global Azure Table Storage Client
var (
	TableClient *aztables.Client
)

// InitAzureTableWithConnectionString initializes the Azure Table Storage connection using a connection string
func InitAzureTableWithConnectionString(connectionString, tableName string) {
	// Create the Azure Table Service client using the connection string
	serviceClient, err := aztables.NewServiceClientFromConnectionString(connectionString, nil)
	if err != nil {
		log.Fatalf("Failed to create Azure Table Service client: %v", err)
	}

	// Ensure the table exists by attempting to create it
	_, err = serviceClient.CreateTable(context.Background(), tableName, nil)
	if err != nil && !isResourceExistsError(err) {
		log.Fatalf("Failed to create table: %v", err)
	}

	// Create a table client for the specified table
	TableClient = serviceClient.NewClient(tableName)

	log.Printf("Successfully connected to Azure Table Storage: Table [%s]", tableName)
}

// isResourceExistsError checks if an error is due to the resource already existing
func isResourceExistsError(err error) bool {
	var responseErr *azcore.ResponseError
	if errors.As(err, &responseErr) {
		return responseErr.StatusCode == http.StatusConflict // 409 Conflict indicates resource exists
	}
	return false
}
