package db

import (
	"context"
	"encoding/json"
	"errors"
	"fmt"
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
}

// InitAzureTables initializes Azure Table Storage connections for all models
func InitAzureTables(connectionString string) {
	TableClients = make(map[string]*aztables.Client)

	// Iterate over each model and create a table
	for _, tableName := range Models {
		initTable(connectionString, tableName)
	}
}

// initTable creates a table if it does not exist and stores the client
func initTable(connectionString, tableName string) {
	// Create the Azure Table Service client using the connection string
	serviceClient, err := aztables.NewServiceClientFromConnectionString(connectionString, nil)
	if err != nil {
		log.Fatalf("Failed to create Azure Table Service client: %v", err)
	}

	// Ensure the table exists by attempting to create it
	_, err = serviceClient.CreateTable(context.Background(), tableName, nil)
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

// QueryTableWithFilter queries the table with a given filter
func QueryTableWithFilter(tableName string, filter string) ([]map[string]interface{}, error) {
	client, exists := TableClients[tableName]
	if !exists {
		return nil, fmt.Errorf("table client for '%s' not initialized", tableName)
	}

	pager := client.NewListEntitiesPager(&aztables.ListEntitiesOptions{
		Filter: &filter,
	})

	var results []map[string]interface{}

	for pager.More() {
		resp, err := pager.NextPage(context.Background())
		if err != nil {
			return nil, err
		}

		for _, entity := range resp.Entities {
			var result map[string]interface{}
			if err := json.Unmarshal(entity, &result); err != nil {
				return nil, err
			}
			results = append(results, result)
		}
	}

	return results, nil
}
