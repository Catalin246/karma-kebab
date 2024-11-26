package repository

import (
	"availability-service-2/models"
	"context"
	"encoding/json"
	"fmt"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
)

type TableStorageAvailabilityRepository struct {
	serviceClient *aztables.ServiceClient
	tableName     string
}

func NewTableStorageAvailabilityRepository(serviceClient *aztables.ServiceClient) *TableStorageAvailabilityRepository {
	return &TableStorageAvailabilityRepository{
		serviceClient: serviceClient,
		tableName:     "Availability",
	}
}

// Create inserts an availability record into Table Storage
func (r *TableStorageAvailabilityRepository) Create(ctx context.Context, availability models.Availability) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Create entity as a map
	entity := map[string]interface{}{
		"PartitionKey": availability.EmployeeID,
		"RowKey":       availability.ID,
		"EmployeeID":   availability.EmployeeID,
		"StartDate":    availability.StartDate.Format(time.RFC3339),
		"EndDate":      availability.EndDate.Format(time.RFC3339),
	}

	// Marshal the entity to JSON
	entityBytes, err := json.Marshal(entity)
	if err != nil {
		return fmt.Errorf("failed to marshal entity: %v", err)
	}

	_, err = tableClient.AddEntity(ctx, entityBytes, nil)
	if err != nil {
		return fmt.Errorf("failed to insert entity: %v", err)
	}

	return nil
}

// GetByEmployeeID retrieves an availability record by ID
// func (r *TableStorageAvailabilityRepository) GetByEmployeeID(ctx context.Context, employeeID string) (*models.Availability, error) {
// 	tableClient := r.serviceClient.NewClient(r.tableName)

// 	response, err := tableClient.GetEntity(ctx, employeeID, nil)
// 	if err != nil {
// 		return nil, fmt.Errorf("failed to get entity: %v", err)
// 	}

// 	// Create a map to hold the entity data
// 	var entityData map[string]interface{}
// 	if err := json.Unmarshal(response.Value, &entityData); err != nil {
// 		return nil, fmt.Errorf("failed to unmarshal entity: %v", err)
// 	}

// 	// Parse the dates
// 	startDate, err := time.Parse(time.RFC3339, entityData["StartDate"].(string))
// 	if err != nil {
// 		return nil, fmt.Errorf("failed to parse start date: %v", err)
// 	}

// 	endDate, err := time.Parse(time.RFC3339, entityData["EndDate"].(string))
// 	if err != nil {
// 		return nil, fmt.Errorf("failed to parse end date: %v", err)
// 	}

// 	// Map entity properties to the Availability model
// 	availability := models.Availability{
// 		EmployeeID: employeeID,
// 		StartDate:  startDate,
// 		EndDate:    endDate,
// 	}

// 	return &availability, nil
// }
func (r *TableStorageAvailabilityRepository) GetByEmployeeID(ctx context.Context, employeeID string) ([]models.Availability, error) {
    tableClient := r.serviceClient.NewClient(r.tableName)

    // Create a filter to get all entities with this partition key
    filter := fmt.Sprintf("PartitionKey eq '%s'", employeeID)
    
    pager := tableClient.NewListEntitiesPager(&aztables.ListEntitiesOptions{
        Filter: &filter,
    })

    var availabilities []models.Availability

    for pager.More() {
        page, err := pager.NextPage(ctx)
        if err != nil {
            return nil, fmt.Errorf("failed to get entities: %v", err)
        }

        for _, entity := range page.Entities {
            var availability models.Availability
            
			err = json.Unmarshal(entity, &availability)
            if err != nil {
                return nil, fmt.Errorf("failed to unmarshal entity: %v", err)
            }
            availabilities = append(availabilities, availability)
        }
    }

    return availabilities, nil
}

// GetAll retrieves all availability records
func (r *TableStorageAvailabilityRepository) GetAll(ctx context.Context, startDate, endDate *time.Time) ([]models.Availability, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Build the base query for Table Storage
	filter := ""

	// Add date filters if startDate and endDate are provided
	if startDate != nil {
		filter += fmt.Sprintf(" and StartDate ge datetime'%s'", startDate.Format("2006-01-02T15:04:05Z"))
	}
	if endDate != nil {
		filter += fmt.Sprintf(" and EndDate le datetime'%s'", endDate.Format("2006-01-02T15:04:05Z"))
	}

	// Create list options with the filter
	listOptions := &aztables.ListEntitiesOptions{
		Filter: &filter,
	}

	// Query table storage with the filter
	pager := tableClient.NewListEntitiesPager(listOptions)
	var availabilities []models.Availability

	for pager.More() {
		response, err := pager.NextPage(ctx)
		if err != nil {
			return nil, fmt.Errorf("failed to list entities: %v", err)
		}

		for _, entityBytes := range response.Entities {
			var entityData map[string]interface{}
			if err := json.Unmarshal(entityBytes, &entityData); err != nil {
				return nil, fmt.Errorf("failed to unmarshal entity: %v", err)
			}

			// Parse the dates
			startDate, err := time.Parse(time.RFC3339, entityData["StartDate"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse start date: %v", err)
			}

			endDate, err := time.Parse(time.RFC3339, entityData["EndDate"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse end date: %v", err)
			}

			availability := models.Availability{
				ID:         entityData["RowKey"].(string),
				EmployeeID: entityData["PartitionKey"].(string),
				StartDate:  startDate,
				EndDate:    endDate,
			}
			availabilities = append(availabilities, availability)
		}
	}

	return availabilities, nil
}

func (r *TableStorageAvailabilityRepository) Update(ctx context.Context, employeeID string, availability models.Availability) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Create entity as a map
	entity := map[string]interface{}{
		"PartitionKey": employeeID,
		"RowKey":       availability.ID,
		"EmployeeID":   availability.EmployeeID,
		"StartDate":    availability.StartDate.Format(time.RFC3339),
		"EndDate":      availability.EndDate.Format(time.RFC3339),
	}

	// Marshal the entity to JSON
	entityBytes, err := json.Marshal(entity)
	if err != nil {
		return fmt.Errorf("failed to marshal entity: %v", err)
	}

	updateOptions := &aztables.UpdateEntityOptions{
		UpdateMode: aztables.UpdateModeReplace,
	}

	_, err = tableClient.UpdateEntity(ctx, entityBytes, updateOptions)
	if err != nil {
		return fmt.Errorf("failed to update entity: %v", err)
	}

	return nil
}

func (r *TableStorageAvailabilityRepository) Delete(ctx context.Context, employeeID string, id string) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	options := &aztables.DeleteEntityOptions{
		IfMatch: nil, // Use nil for unconditional delete
	}

	_, err := tableClient.DeleteEntity(ctx, employeeID, id, options)
	if err != nil {
		return fmt.Errorf("failed to delete availability record with id %s for employee %s: %v", id, employeeID, err)
	}

	return nil
}
