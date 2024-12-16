package db

import (
	"context"
	"errors"
	"fmt"
	"io"
	"log"
	"net/http"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/Azure/azure-sdk-for-go/sdk/storage/azblob"
)

// Global Azure Table Storage Client
var (
	TableClients map[string]*aztables.Client // Store table clients for each table
)

// Models defines the list of tables to be created
var Models = []string{
	"duties", "dutyAssignments",
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

//////////////////////////////////////

// Azure Blob for Duty Assignment images:

// Global Azure Blob Storage client
var BlobServiceClient *azblob.Client

// InitAzureBlobStorage initializes the Azure Blob Storage client
func InitAzureBlobStorage(connectionString string) (*azblob.Client, error) {
	client, err := azblob.NewClientFromConnectionString(connectionString, nil)
	if err != nil {
		log.Fatalf("Failed to create Azure Blob Storage client: %v", err)
		return nil, err
	}

	BlobServiceClient = client //TODO check
	log.Println("Successfully connected to Azure Blob Storage")
	return BlobServiceClient, nil
}

// UploadImage uploads an image to the specified container and returns the URL
func UploadImage(ctx context.Context, containerName string, blobName string, imageData io.Reader) (string, error) {
	// Ensure the container exists (create if not)
	_, err := BlobServiceClient.CreateContainer(ctx, containerName, nil)
	if err != nil && !isContainerExistsError(err) {
		return "", fmt.Errorf("failed to create blob container: %v", err)
	}

	// Upload the image to the blob storage
	_, err = BlobServiceClient.UploadStream(ctx, containerName, blobName, imageData, nil)
	if err != nil {
		return "", fmt.Errorf("failed to upload image: %v", err)
	}

	// Construct the blob URL
	blobURL := fmt.Sprintf("%s/%s/%s", BlobServiceClient.URL(), containerName, blobName)
	return blobURL, nil
}

// isContainerExistsError checks if an error is due to the container already existing
func isContainerExistsError(err error) bool {
	var responseErr *azcore.ResponseError
	if errors.As(err, &responseErr) {
		return responseErr.StatusCode == http.StatusConflict // 409 Conflict indicates container exists
	}
	return false
}
