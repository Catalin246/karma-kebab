package repositories

import (
	"context"
	"encoding/json"
	"fmt"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/google/uuid"

	"duty-service/models"
)

// DutyRepository implements DutyStorage using Azure Table Storage
type DutyRepository struct {
	client *aztables.Client
	table  string
}

// NewDutyRepository creates a new instance of DutyRepository using a connection string
func NewDutyRepository(connectionString, tableName string) (*DutyRepository, error) {
	// Create a new client for Azure Tables using the connection string
	serviceClient, err := aztables.NewServiceClientFromConnectionString(connectionString, nil)
	if err != nil {
		return nil, fmt.Errorf("failed to create Azure Table ServiceClient from connection string: %w", err)
	}

	// Now create a Client for the specific table
	client := serviceClient.NewClient(tableName)

	return &DutyRepository{
		client: client,
		table:  tableName,
	}, nil
}

func (s *DutyRepository) GetAllDuties() ([]models.Duty, error) {
	var duties []models.Duty

	// Setup the options for listing entities
	options := &aztables.ListEntitiesOptions{
		Top: nil, // To limit the number of records per request CHECKLATER
	}

	// Initialize the pager for listing entities
	pager := s.client.NewListEntitiesPager(options)

	// Iterate through the pages of entities
	for pager.More() {
		page, err := pager.NextPage(context.Background())
		if err != nil {
			return nil, fmt.Errorf("failed to list duties: %w", err)
		}

		// Iterate through the entities in the current page
		for _, entity := range page.Entities {
			// Unmarshal the entity bytes into an EDMEntity
			var edmEntity aztables.EDMEntity
			err := json.Unmarshal(entity, &edmEntity)
			if err != nil {
				return nil, fmt.Errorf("failed to unmarshal entity: %w", err)
			}

			//DUTY ID
			idVal, exists := edmEntity.Properties["id"]
			if !exists {
				return nil, fmt.Errorf("missing 'id' property")
			}

			idStr, ok := idVal.(string)
			if !ok {
				return nil, fmt.Errorf("invalid type for 'id' property: expected string, got %T", idVal)
			}

			id, err := uuid.Parse(idStr)
			if err != nil {
				return nil, fmt.Errorf("invalid UUID format: %w", err)
			}

			//DUTY ROLE ID
			roleIdVal, exists := edmEntity.Properties["role_id"]
			if !exists {
				return nil, fmt.Errorf("missing 'role_id' property")
			}

			roleIdStr, ok := roleIdVal.(string)
			if !ok {
				return nil, fmt.Errorf("invalid type for 'role_id' property: expected string, got %T", roleIdVal)
			}

			roleId, err := uuid.Parse(roleIdStr)
			if err != nil {
				return nil, fmt.Errorf("invalid UUID format for 'role_id': %w", err)
			}

			//DUTY NAME:
			nameVal, exists := edmEntity.Properties["name"]
			if !exists {
				return nil, fmt.Errorf("missing 'name' property")
			}

			name, ok := nameVal.(string)
			if !ok {
				return nil, fmt.Errorf("invalid type for 'name' property: expected string, got %T", nameVal)
			}

			// DUTY DESC:
			descriptionVal, exists := edmEntity.Properties["description"]
			if !exists {
				return nil, fmt.Errorf("missing 'description' property")
			}

			description, ok := descriptionVal.(string)
			if !ok {
				return nil, fmt.Errorf("invalid type for 'description' property: expected string, got %T", descriptionVal)
			}

			// Create the Duty model from the entity properties
			duty := models.Duty{
				Id:              id.String(),
				RoleId:          roleId.String(),
				DutyName:        name,
				DutyDescription: description,
			}

			// Append the duty to the duties slice
			duties = append(duties, duty)
		}
	}

	return duties, nil
}
