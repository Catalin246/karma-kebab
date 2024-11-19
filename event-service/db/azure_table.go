package db

import (
	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
)

// Global Azure Table Storage Client
var (
	TableClient *aztables.Client
)

// InitAzureTable initialize the Azure Table Storage connection
func InitAzureTable(accountName, accountKey, tableName string) {
	// TO DO
}
