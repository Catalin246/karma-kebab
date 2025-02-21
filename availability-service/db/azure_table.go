package db

import (
	"context"
	"errors"
	"log"
	"net/http"
	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
)

var (
	TableClients map[string]*aztables.Client 
)

// Models defines the list of tables to be created
var Models = []string{
	"availability",
}

func InitAzureTables(connectionString string) (*aztables.ServiceClient, error) {
	TableClients = make(map[string]*aztables.Client)

	serviceClient, err := aztables.NewServiceClientFromConnectionString(connectionString, nil)
	if err != nil {
		log.Fatalf("Failed to create Azure Table Service client: %v", err)
		return nil, err
	}

	for _, tableName := range Models {
		initTable(serviceClient, tableName)
	}

	return serviceClient, nil
}

func initTable(serviceClient *aztables.ServiceClient, tableName string) {
	_, err := serviceClient.CreateTable(context.Background(), tableName, nil)
	if err != nil && !isResourceExistsError(err) {
		log.Fatalf("Failed to create table [%s]: %v", tableName, err)
	}

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
