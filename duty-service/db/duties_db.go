package db

import (
	"context"
	"errors"
	"fmt"
	"io"
	"log"
	"net/http"
	"strings"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/Azure/azure-sdk-for-go/sdk/storage/azblob"
	"github.com/Azure/azure-sdk-for-go/sdk/storage/azblob/sas"
)

// Global Azure Table Storage Client
var (
	BlobServiceClient *azblob.Client
	AccountName       string
	AccountKey        string
	TableClients      map[string]*aztables.Client // Store table clients for each table
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

// InitAzureBlobStorage initializes the Azure Blob Storage client
// InitAzureBlobStorage initializes the Azure Blob Storage client and sets account details
func InitAzureBlobStorage(connectionString, accountName, accountKey string) (*azblob.Client, error) {
	client, err := azblob.NewClientFromConnectionString(connectionString, nil)
	if err != nil {
		log.Fatalf("Failed to create Azure Blob Storage client: %v", err)
		return nil, err
	}

	BlobServiceClient = client
	AccountName = accountName
	AccountKey = accountKey

	log.Println("Successfully connected to Azure Blob Storage")
	return BlobServiceClient, nil
}

// UploadImage uploads an image to the specified container, returns the SAS URL
func UploadImage(ctx context.Context, containerName string, blobName string, imageData io.Reader) (string, error) {
	_, err := BlobServiceClient.CreateContainer(ctx, containerName, nil)
	if err != nil && !isContainerExistsError(err) {
		return "", fmt.Errorf("failed to create blob container: %v", err)
	}

	_, err = BlobServiceClient.UploadStream(ctx, containerName, blobName, imageData, nil)
	if err != nil {
		return "", fmt.Errorf("failed to upload image: %v", err)
	}

	sasURL, err := GenerateSASURL(containerName, blobName, time.Hour*24*90) // 3 months expiry
	if err != nil {
		return "", fmt.Errorf("failed to generate SAS URL: %v", err)
	}

	return sasURL, nil
}

// GenerateSASURL generates a SAS URL for a blob with the specified expiry duration
func GenerateSASURL(containerName string, blobName string, expiry time.Duration) (string, error) {
	permissions := sas.BlobPermissions{Read: true}
	expiryTime := time.Now().Add(expiry)

	// Default to HTTPS
	protocol := "https"

	// If using Azurite (local development), set protocol to HTTP
	if strings.HasPrefix(AccountName, "devstoreaccount1") {
		protocol = "http"
	}

	// Ensure that there is no double slash in the base URL
	baseURL := fmt.Sprintf("%s://%s.blob.core.windows.net/%s", protocol, AccountName, containerName)
	blobBaseURL := fmt.Sprintf("%s/%s", baseURL, blobName) // Ensure no extra slashes

	// Set up SAS Blob Signature Values with proper protocol handling
	sasValues := sas.BlobSignatureValues{
		Protocol:      sas.Protocol(protocol),
		ExpiryTime:    expiryTime,
		ContainerName: containerName,
		BlobName:      blobName,
		Permissions:   permissions.String(),
	}

	cred, err := azblob.NewSharedKeyCredential(AccountName, AccountKey)
	if err != nil {
		return "", fmt.Errorf("failed to create shared key credential: %v", err)
	}

	// Generate the SAS query parameters
	sasQueryParams, err := sasValues.SignWithSharedKey(cred)
	if err != nil {
		return "", fmt.Errorf("failed to sign SAS values: %v", err)
	}

	// Now we append the SAS token to the base blob URL
	blobURL := fmt.Sprintf("%s?%s", blobBaseURL, sasQueryParams.Encode())

	return blobURL, nil
}

// isContainerExistsError checks if an error is due to the container already existing
func isContainerExistsError(err error) bool {
	var responseErr *azcore.ResponseError
	if errors.As(err, &responseErr) {
		return responseErr.StatusCode == http.StatusConflict
	}
	return false
}
