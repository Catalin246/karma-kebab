package repositories

import (
	"context"
	"duty-service/models"
	"encoding/json"
	"fmt"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/google/uuid"
)

type DutyAssignmentRepository struct {
	serviceClient *aztables.ServiceClient
	tableName     string
}

func NewDutyAssignmentRepository(serviceClient *aztables.ServiceClient) *DutyAssignmentRepository {
	return &DutyAssignmentRepository{
		serviceClient: serviceClient,
		tableName:     "dutyAssignments",
	}
}

// GET ALL DUTY ASSIGNMENTS BY SHIFT ID
func (r *DutyAssignmentRepository) GetAllDutyAssignmentsByShiftId(ctx context.Context, shiftId uuid.UUID) ([]models.DutyAssignment, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Construct the filter to match the ShiftId
	filter := fmt.Sprintf("PartitionKey eq '%s'", shiftId.String())

	listOptions := &aztables.ListEntitiesOptions{
		Filter: &filter,
	}

	pager := tableClient.NewListEntitiesPager(listOptions)

	var dutyAssignments []models.DutyAssignment

	// Loop through pages of duty assignments
	for pager.More() {
		page, err := pager.NextPage(ctx)
		if err != nil {
			return nil, fmt.Errorf("failed to list duty assignments by ShiftId: %v", err)
		}

		// Unmarshal each entity and add it to the list
		for _, entity := range page.Entities {
			var dutyAssignmentData map[string]interface{}

			if err := json.Unmarshal(entity, &dutyAssignmentData); err != nil {
				return nil, fmt.Errorf("failed to unmarshal duty assignment: %v", err)
			}

			// Parse the optional fields (nullable fields like image URL and note)
			var dutyAssignmentImageUrl *string
			if imageUrl, ok := dutyAssignmentData["DutyAssignmentImageUrl"].(string); ok && imageUrl != "" {
				dutyAssignmentImageUrl = &imageUrl
			}

			var dutyAssignmentNote *string
			if note, ok := dutyAssignmentData["DutyAssignmentNote"].(string); ok && note != "" {
				dutyAssignmentNote = &note
			}

			// Create the DutyAssignment struct
			dutyAssignment := models.DutyAssignment{
				PartitionKey:           uuid.MustParse(dutyAssignmentData["PartitionKey"].(string)),
				RowKey:                 uuid.MustParse(dutyAssignmentData["RowKey"].(string)),
				DutyAssignmentStatus:   models.DutyAssignmentStatus(dutyAssignmentData["DutyAssignmentStatus"].(string)),
				DutyAssignmentImageUrl: dutyAssignmentImageUrl,
				DutyAssignmentNote:     dutyAssignmentNote,
			}

			dutyAssignments = append(dutyAssignments, dutyAssignment)
		}
	}

	return dutyAssignments, nil
}

// POST - creates duty assignments for a Shift
func (r *DutyAssignmentRepository) CreateDutyAssignments(ctx context.Context, shiftId uuid.UUID, duties []models.Duty) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	for _, duty := range duties {
		dutyAssignment := models.DutyAssignment{
			PartitionKey:           shiftId,                 // Use ShiftId as PartitionKey
			RowKey:                 uuid.New(),              // Generate new UUID for DutyId TODO: make unique
			DutyAssignmentStatus:   models.StatusIncomplete, // Default to Incomplete
			DutyAssignmentImageUrl: nil,                     // No image URL on creation
			DutyAssignmentNote:     nil,                     // No note on creation
		}

		// Marshal the DutyAssignment into a JSON entity
		entity := map[string]interface{}{
			"PartitionKey":           dutyAssignment.PartitionKey.String(),
			"RowKey":                 dutyAssignment.RowKey.String(),
			"DutyAssignmentStatus":   string(dutyAssignment.DutyAssignmentStatus),
			"DutyAssignmentImageUrl": dutyAssignment.DutyAssignmentImageUrl,
			"DutyAssignmentNote":     dutyAssignment.DutyAssignmentNote,
		}

		entityBytes, err := json.Marshal(entity)
		if err != nil {
			return fmt.Errorf("failed to marshal duty assignment: %v", err)
		}

		// Insert the entity into Azure Table Storage
		_, err = tableClient.AddEntity(ctx, entityBytes, nil)
		if err != nil {
			return fmt.Errorf("failed to create duty assignment for DutyId %s: %v", duty.RowKey, err)
		}
	}

	return nil
}

// UPDATE a duty assignment
func (r *DutyAssignmentRepository) UpdateDutyAssignment(ctx context.Context, dutyAssignment models.DutyAssignment) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Prepare the updated entity
	entity := map[string]interface{}{
		"PartitionKey":           dutyAssignment.PartitionKey.String(),
		"RowKey":                 dutyAssignment.RowKey.String(),
		"DutyAssignmentStatus":   string(dutyAssignment.DutyAssignmentStatus),
		"DutyAssignmentImageUrl": dutyAssignment.DutyAssignmentImageUrl,
		"DutyAssignmentNote":     dutyAssignment.DutyAssignmentNote,
	}

	// Marshal the entity to JSON
	entityBytes, err := json.Marshal(entity)
	if err != nil {
		return fmt.Errorf("failed to marshal updated entity: %v", err)
	}

	// Update the entity in Azure Table Storage
	_, err = tableClient.UpdateEntity(ctx, entityBytes, nil)
	if err != nil {
		return fmt.Errorf("failed to update duty assignment: %v", err)
	}

	return nil
}

// DELETE a duty assignment by ShiftId and DutyId
func (r *DutyAssignmentRepository) DeleteDutyAssignment(ctx context.Context, shiftId uuid.UUID, dutyId uuid.UUID) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Prepare the PartitionKey and RowKey
	partitionKey := shiftId.String() // ShiftId
	rowKey := dutyId.String()        // DutyId

	// Delete the entity in Azure Table Storage
	_, err := tableClient.DeleteEntity(ctx, partitionKey, rowKey, nil)
	if err != nil {
		return fmt.Errorf("failed to delete duty assignment: %v", err)
	}

	return nil
}
