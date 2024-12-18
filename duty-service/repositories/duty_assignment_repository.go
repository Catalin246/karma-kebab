package repositories

import (
	"context"
	"duty-service/db"
	"duty-service/models"
	"encoding/json"
	"fmt"
	"io"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/Azure/azure-sdk-for-go/sdk/storage/azblob"
	"github.com/google/uuid"
)

type DutyAssignmentRepository struct {
	blobClient    *azblob.Client
	serviceClient *aztables.ServiceClient
	tableName     string
}

func NewDutyAssignmentRepository(serviceClient *aztables.ServiceClient, blobClient *azblob.Client) *DutyAssignmentRepository {
	return &DutyAssignmentRepository{
		blobClient:    blobClient,
		serviceClient: serviceClient,
		tableName:     "dutyAssignments",
	}
}

// GET ALL DUTY ASSIGNMENTS BY SHIFT ID
func (r *DutyAssignmentRepository) GetAllDutyAssignmentsByShiftId(ctx context.Context, shiftId uuid.UUID) ([]models.DutyAssignment, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)

	filter := fmt.Sprintf("PartitionKey eq '%s'", shiftId.String()) // filter to match the ShiftId

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

		// unmarshal entities and add it to the list
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

			// Create the DutyAssignment
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
			PartitionKey:           shiftId,
			RowKey:                 uuid.New(),              // generate a new UUID for the RowKey (it's EXTREMELY unlikely for new generated UUIDS to collide with existing ones. source: https://stackoverflow.com/questions/24876188/how-big-is-the-chance-to-get-a-java-uuid-randomuuid-collision)
			DutyAssignmentStatus:   models.StatusIncomplete, // default: Incomplete
			DutyAssignmentImageUrl: nil,                     // no image URL on creation
			DutyAssignmentNote:     nil,                     // no note on creation
		}

		// marshal to a json
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

		_, err = tableClient.AddEntity(ctx, entityBytes, nil) // Insert the entity into Azure Table Storage
		if err != nil {
			return fmt.Errorf("failed to create duty assignment for DutyId %s: %v", duty.RowKey, err)
		}
	}

	return nil
}

// UPDATE a duty assignment
func (r *DutyAssignmentRepository) UpdateDutyAssignment(ctx context.Context, dutyAssignment models.DutyAssignment, image io.Reader) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// If there's an image to upload, handle the upload and get the URL
	if image != nil {
		//blobName := fmt.Sprintf("%s/%s.png", dutyAssignment.PartitionKey.String(), dutyAssignment.RowKey.String()) //folder structure
		blobName := fmt.Sprintf("%s_%s.png", dutyAssignment.PartitionKey.String(), dutyAssignment.RowKey.String()) // no folder structure, just a unique name. [shiftid]_[dutyid].png
		imageURL, err := db.UploadImage(ctx, "dutyassignmentimages", blobName, image)
		if err != nil {
			return fmt.Errorf("failed to upload image: %v", err)
		}
		dutyAssignment.DutyAssignmentImageUrl = &imageURL
	}

	// preparing the updated entity (only include fields that are non-nil or non-empty)
	entity := map[string]interface{}{
		"PartitionKey": dutyAssignment.PartitionKey.String(),
		"RowKey":       dutyAssignment.RowKey.String(),
	}

	// add fields if they have values
	if dutyAssignment.DutyAssignmentStatus != "" {
		entity["DutyAssignmentStatus"] = string(dutyAssignment.DutyAssignmentStatus)
	}

	if dutyAssignment.DutyAssignmentImageUrl != nil && *dutyAssignment.DutyAssignmentImageUrl != "" {
		entity["DutyAssignmentImageUrl"] = dutyAssignment.DutyAssignmentImageUrl
	}

	if dutyAssignment.DutyAssignmentNote != nil && *dutyAssignment.DutyAssignmentNote != "" {
		entity["DutyAssignmentNote"] = dutyAssignment.DutyAssignmentNote
	}

	entityBytes, err := json.Marshal(entity) // marshal to JSON
	if err != nil {
		return fmt.Errorf("failed to marshal updated entity: %v", err)
	}

	// using Merge Update to avoid overwriting other fields!!!
	_, err = tableClient.UpdateEntity(ctx, entityBytes, &aztables.UpdateEntityOptions{UpdateMode: aztables.UpdateModeMerge}) // aztables.UpdateModeMerge specifies a merge update
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

	_, err := tableClient.DeleteEntity(ctx, partitionKey, rowKey, nil) // Delete the entity in Azure Table Storage
	if err != nil {
		return fmt.Errorf("failed to delete duty assignment: %v", err)
	}

	return nil
}