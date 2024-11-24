package repository

import (
	"availability-service/models"
	"context"
	"fmt"
	"time"
	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
)

type TableStorageAvailabilityRepository struct {
	tableClient *aztable.Client
}

func NewTableStorageAvailabilityRepository(tableClient *aztable.Client) *TableStorageAvailabilityRepository {
	return &TableStorageAvailabilityRepository{
		tableClient: tableClient,
	}
}

// Create inserts an availability record into Table Storage
func (r *TableStorageAvailabilityRepository) Create(ctx context.Context, availability models.Availability) error {
	table := r.tableClient.NewTable("Availability") // Your table name
	entity := aztable.Ent{
		PartitionKey: aztable.String("availability"),  // Partition key
		RowKey:       aztable.String(availability.ID), // Row key (unique identifier)
		Properties: map[string]interface{}{
			"EmployeeID": availability.EmployeeID,
			"StartDate":  availability.StartDate,
			"EndDate":    availability.EndDate,
		},
	}

	_, err := table.InsertEntity(ctx, &entity)
	if err != nil {
		return fmt.Errorf("failed to insert entity: %v", err)
	}

	return nil
}

// GetByID retrieves an availability record by ID
func (r *TableStorageAvailabilityRepository) GetByID(ctx context.Context, employeeID, id string) (*models.Availability, error) {
	table := r.tableClient.NewTable("Availability")

	// Get the entity using the partition key (employeeID) and row key (id)
	entity, err := table.GetEntity(ctx, employeeID, id)
	if err != nil {
		return nil, fmt.Errorf("failed to get entity: %v", err)
	}

	// Map entity properties to the Availability model
	availability := models.Availability{
		ID:         id,
		EmployeeID: employeeID,
		StartDate:  entity.Properties["StartDate"].(string),
		EndDate:    entity.Properties["EndDate"].(string),
	}

	return &availability, nil
}

// GetAll retrieves all availability records
func (r *TableStorageAvailabilityRepository) GetAll(ctx context.Context, employeeID string, startDate, endDate *time.Time) ([]models.Availability, error) {
	// Build the base query for Table Storage
	filter := fmt.Sprintf("PartitionKey eq '%s'", employeeID)

	// Add date filters if startDate and endDate are provided
	if startDate != nil {
		filter += fmt.Sprintf(" and Date ge '%s'", startDate.Format(time.RFC3339))
	}
	if endDate != nil {
		filter += fmt.Sprintf(" and Date le '%s'", endDate.Format(time.RFC3339))
	}

	// Query table storage with the filter
	entities, err := r.tableClient.QueryEntities(ctx, "Availability", filter, nil)
	if err != nil {
		return nil, fmt.Errorf("failed to query availability records: %w", err)
	}

	// Convert entities to Availability models
	var availabilities []models.Availability
	for _, entity := range entities {
		var availability models.Availability
		if err := entity.Unmarshal(&availability); err != nil {
			return nil, fmt.Errorf("failed to unmarshal entity: %v", err)
		}
		availabilities = append(availabilities, availability)
	}

	return availabilities, nil
}

func (r *TableStorageAvailabilityRepository) Update(ctx context.Context, employeeID string, availability models.Availability) error {
	table := r.tableClient.NewTable("Availability")

	entity := aztable.Ent{
		PartitionKey: aztable.String(employeeID),      // Using employeeID as partition key
		RowKey:       aztable.String(availability.ID), // Using availability ID as row key
		Properties: map[string]interface{}{
			"EmployeeID": availability.EmployeeID,
			"StartDate":  availability.StartDate,
			"EndDate":    availability.EndDate,
		},
	}

	// Update the entity in the table
	_, err := table.UpdateEntity(ctx, &entity, aztable.Replace)
	if err != nil {
		return fmt.Errorf("failed to update entity: %v", err)
	}

	return nil
}

func (r *TableStorageAvailabilityRepository) Delete(ctx context.Context, employeeID string, id string) error {
	table := r.tableClient.NewTable("Availability")

	partitionKey := employeeID // Use employeeID as partition key
	rowKey := id               // Use id as row key

	_, err := table.DeleteEntity(ctx, partitionKey, rowKey)
	if err != nil {
		return fmt.Errorf("failed to delete availability recod with id %s for employee %s: %v", id, employeeID, err)
	}

	return nil
}
