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
func (r *DutyRepository) GetAllDutyAssignmentsByShiftId(ctx context.Context, shiftId uuid.UUID) ([]models.DutyAssignment, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Construct the filter to match the ShiftId
	filter := fmt.Sprintf("PartitionKey eq '%s'", shiftId.String())
	fmt.Printf("Filter: %s\n", filter) // Debug the filter

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

			// Parse the DutyAssignment properties
			dutyIdUUID, err := uuid.Parse(dutyAssignmentData["DutyId"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse DutyId as UUID: %v", err)
			}

			shiftIdUUID, err := uuid.Parse(dutyAssignmentData["ShiftId"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse ShiftId as UUID: %v", err)
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
				PartitionKey:           dutyAssignmentData["PartitionKey"].(string),
				RowKey:                 dutyAssignmentData["RowKey"].(string),
				DutyId:                 dutyIdUUID,
				ShiftId:                shiftIdUUID,
				DutyAssignmentStatus:   models.DutyAssignmentStatus(dutyAssignmentData["DutyAssignmentStatus"].(string)),
				DutyAssignmentImageUrl: dutyAssignmentImageUrl,
				DutyAssignmentNote:     dutyAssignmentNote,
			}

			dutyAssignments = append(dutyAssignments, dutyAssignment)
		}
	}

	return dutyAssignments, nil
}
