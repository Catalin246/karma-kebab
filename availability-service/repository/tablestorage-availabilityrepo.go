package repository

import (
	"availability-service/models"
	"context"
	"encoding/json"
	"fmt"
	"strings"
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

func (r *TableStorageAvailabilityRepository) Create(ctx context.Context, availability models.Availability) error {
    // checks for existing overlapping availabilities
    existingAvailabilities, err := r.GetOverlappingAvailabilities(ctx, availability)
    if err != nil {
        return fmt.Errorf("failed to check existing availabilities: %v", err)
    }

    if len(existingAvailabilities) > 0 {
        return fmt.Errorf("availability conflicts with existing entries")
    }

    tableClient := r.serviceClient.NewClient(r.tableName)

    // Create entity as a map
    entity := map[string]interface{}{
        "PartitionKey": availability.EmployeeID,
        "RowKey":       availability.ID,
        "EmployeeID":   availability.EmployeeID,
        "StartDate":    availability.StartDate.Format(time.RFC3339),
        "EndDate":      availability.EndDate.Format(time.RFC3339),
    }

    // entity to JSON
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

func (r *TableStorageAvailabilityRepository) GetOverlappingAvailabilities(ctx context.Context, availability models.Availability) ([]models.Availability, error) {
    filter := fmt.Sprintf("PartitionKey eq '%s' and ((StartDate le datetime'%s' and EndDate ge datetime'%s') or (StartDate le datetime'%s' and EndDate ge datetime'%s') or (StartDate ge datetime'%s' and StartDate le datetime'%s'))", 
        availability.EmployeeID,
        availability.StartDate.Format("2006-01-02T15:04:05Z"),
        availability.StartDate.Format("2006-01-02T15:04:05Z"),
        availability.EndDate.Format("2006-01-02T15:04:05Z"),
        availability.EndDate.Format("2006-01-02T15:04:05Z"),
        availability.StartDate.Format("2006-01-02T15:04:05Z"),
        availability.EndDate.Format("2006-01-02T15:04:05Z"))

    listOptions := &aztables.ListEntitiesOptions{
        Filter: &filter,
    }

    tableClient := r.serviceClient.NewClient(r.tableName)
    pager := tableClient.NewListEntitiesPager(listOptions)
    
    var overlappingAvailabilities []models.Availability

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

            // Exclude the current availability being created (if it has an ID)
            if entityData["RowKey"].(string) != availability.ID {
                overlappingAvailabilities = append(overlappingAvailabilities, models.Availability{
                    ID:         entityData["RowKey"].(string),
                    EmployeeID: entityData["PartitionKey"].(string),
                    StartDate:  startDate,
                    EndDate:    endDate,
                })
            }
        }
    }

    return overlappingAvailabilities, nil
}


func (r *TableStorageAvailabilityRepository) GetAll(ctx context.Context, employeeID string, startDate, endDate *time.Time) ([]models.Availability, error) {
    tableClient := r.serviceClient.NewClient(r.tableName)

    var filterParts []string

    if employeeID != "" {
        filterParts = append(filterParts, fmt.Sprintf("PartitionKey eq '%s'", employeeID))
    }

    if startDate != nil && endDate != nil {
        datePart := fmt.Sprintf("(StartDate ge datetime'%s' and StartDate le datetime'%s') or (EndDate ge datetime'%s' and EndDate le datetime'%s') or (StartDate le datetime'%s' and EndDate ge datetime'%s')",
            startDate.Format("2006-01-02T15:04:05Z"),
            endDate.Format("2006-01-02T15:04:05Z"),
            startDate.Format("2006-01-02T15:04:05Z"),
            endDate.Format("2006-01-02T15:04:05Z"),
            startDate.Format("2006-01-02T15:04:05Z"),
            endDate.Format("2006-01-02T15:04:05Z"))
        filterParts = append(filterParts, fmt.Sprintf("(%s)", datePart))
    } else if startDate != nil {
        filterParts = append(filterParts, fmt.Sprintf("StartDate ge datetime'%s'", startDate.Format("2006-01-02T15:04:05Z")))
    } else if endDate != nil {
        filterParts = append(filterParts, fmt.Sprintf("EndDate le datetime'%s'", endDate.Format("2006-01-02T15:04:05Z")))
    }

    var filter string
    if len(filterParts) > 0 {
        filter = strings.Join(filterParts, " and ")
    }

    var listOptions *aztables.ListEntitiesOptions
    if filter != "" {
        listOptions = &aztables.ListEntitiesOptions{
            Filter: &filter,
        }
    }

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

	// entity to JSON
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
