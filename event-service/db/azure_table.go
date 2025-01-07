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
	TableClients map[string]*aztables.Client // Store table clients for each table
)

// Models defines the list of tables to be created
var Models = []string{
	"events",
	"persons",
	"eventshifts",
}

// InitAzureTables initializes Azure Table Storage connections for all models
func InitAzureTables(connectionString string) (*aztables.ServiceClient, error) {
	TableClients = make(map[string]*aztables.Client)

	// Create the Azure Table Service client
	serviceClient, err := aztables.NewServiceClientFromConnectionString(connectionString, nil)
	if err != nil {
		log.Fatalf("Failed to create Azure Table Service client: %v", err)
		return nil, err
	}

	// Iterate over each model and create a table
	for _, tableName := range Models {
		initTable(serviceClient, tableName)
	}

	// Return the service client for later use
	return serviceClient, nil
}

// initTable creates a table if it does not exist and stores the client
func initTable(serviceClient *aztables.ServiceClient, tableName string) {
	// Ensure the table exists by attempting to create it
	_, err := serviceClient.CreateTable(context.Background(), tableName, nil)
	if err != nil && !isResourceExistsError(err) {
		log.Fatalf("Failed to create table [%s]: %v", tableName, err)
	}

	// Create a table client for the specified table
	TableClients[tableName] = serviceClient.NewClient(tableName)

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
