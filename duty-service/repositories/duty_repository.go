package repositories

import (
	"context"
	"duty-service/models"
	"encoding/json"
	"fmt"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/google/uuid"
)

type DutyRepository struct {
	serviceClient *aztables.ServiceClient
	tableName     string
}

func NewDutyRepository(serviceClient *aztables.ServiceClient) *DutyRepository {
	return &DutyRepository{
		serviceClient: serviceClient,
		tableName:     "duties",
	}
}

// GET ALL DUTIES
func (r *DutyRepository) GetAllDuties(ctx context.Context, filter string) ([]models.Duty, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)

	listOptions := &aztables.ListEntitiesOptions{
		Filter: &filter,
	}

	pager := tableClient.NewListEntitiesPager(listOptions)

	var duties []models.Duty

	for pager.More() {
		page, err := pager.NextPage(ctx)
		if err != nil {
			return nil, fmt.Errorf("failed to list duties: %v", err)
		}

		for _, entity := range page.Entities {
			var dutyData map[string]interface{}

			if err := json.Unmarshal(entity, &dutyData); err != nil {
				return nil, fmt.Errorf("failed to unmarshal duty: %v", err)
			}

			duty, err := parseDuty(dutyData)
			if err != nil {
				return nil, err
			}

			duties = append(duties, duty)
		}
	}
	return duties, nil
}

// GET DUTY BY ID (PartitionKey and RowKey)
func (r *DutyRepository) GetDutyById(ctx context.Context, partitionKey, rowKey string) (*models.Duty, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)

	resp, err := tableClient.GetEntity(ctx, partitionKey, rowKey, nil)
	if err != nil {
		return nil, fmt.Errorf("failed to get duty: %v", err)
	}

	var dutyData map[string]interface{}
	if err := json.Unmarshal(resp.Value, &dutyData); err != nil {
		return nil, fmt.Errorf("failed to decode duty: %v", err)
	}

	duty, err := parseDuty(dutyData)
	if err != nil {
		return nil, err
	}

	return &duty, nil
}

// GET DUTIES BY ROLE ID (using RoleId as filter)
func (r *DutyRepository) GetDutiesByRole(ctx context.Context, roleId int) ([]models.Duty, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)

	filter := fmt.Sprintf("RoleId eq %d", roleId) // roleId is now an int

	listOptions := &aztables.ListEntitiesOptions{
		Filter: &filter,
	}

	pager := tableClient.NewListEntitiesPager(listOptions)

	var duties []models.Duty

	for pager.More() {
		page, err := pager.NextPage(ctx)
		if err != nil {
			return nil, fmt.Errorf("failed to list duties by RoleId: %v", err)
		}

		for _, entity := range page.Entities {
			var dutyData map[string]interface{}

			if err := json.Unmarshal(entity, &dutyData); err != nil {
				return nil, fmt.Errorf("failed to unmarshal duty: %v", err)
			}

			duty, err := parseDuty(dutyData)
			if err != nil {
				return nil, err
			}

			duties = append(duties, duty)
		}
	}

	return duties, nil
}

// CREATE NEW DUTY (POST)
func (r *DutyRepository) CreateDuty(ctx context.Context, duty models.Duty) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Prepare the entity for insertion
	entity := map[string]interface{}{
		"PartitionKey":    duty.PartitionKey,
		"RowKey":          duty.RowKey.String(),
		"RoleId":          duty.RoleId,
		"DutyName":        duty.DutyName,
		"DutyDescription": duty.DutyDescription,
	}

	// Marshal the entity to JSON
	entityBytes, err := json.Marshal(entity)
	if err != nil {
		return fmt.Errorf("failed to marshal entity: %v", err)
	}

	// Insert the entity
	_, err = tableClient.AddEntity(ctx, entityBytes, nil)
	if err != nil {
		return fmt.Errorf("failed to insert duty: %v", err)
	}

	return nil
}

// UPDATE A DUTY (PUT)
func (r *DutyRepository) UpdateDuty(ctx context.Context, partitionKey, rowKey string, duty models.Duty) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Prepare the updated entity
	entity := map[string]interface{}{
		"PartitionKey":    duty.PartitionKey,
		"RowKey":          duty.RowKey.String(),
		"RoleId":          duty.RoleId,
		"DutyName":        duty.DutyName,
		"DutyDescription": duty.DutyDescription,
	}

	entityBytes, err := json.Marshal(entity)
	if err != nil {
		return fmt.Errorf("failed to marshal updated entity: %v", err)
	}

	// Update the entity
	_, err = tableClient.UpdateEntity(ctx, entityBytes, nil)
	if err != nil {
		return fmt.Errorf("failed to update duty: %v", err)
	}

	return nil
}

// DELETE A DUTY (removes a duty by PartitionKey and RowKey)
func (r *DutyRepository) DeleteDuty(ctx context.Context, partitionKey, rowKey string) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Delete the entity
	_, err := tableClient.DeleteEntity(ctx, partitionKey, rowKey, nil)
	if err != nil {
		return fmt.Errorf("failed to delete duty: %v", err)
	}

	return nil
}

// parseDuty is a helper function to parse duty data into a models.Duty object.
func parseDuty(dutyData map[string]interface{}) (models.Duty, error) {
	// Parse RowKey as UUID
	rowKeyUUID, err := uuid.Parse(dutyData["RowKey"].(string))
	if err != nil {
		return models.Duty{}, fmt.Errorf("failed to parse RowKey as UUID: %v", err)
	}

	// Parse RoleId as int //TODO check if this is correct
	roleIdFloat, ok := dutyData["RoleId"].(float64)
	if !ok {
		return models.Duty{}, fmt.Errorf("failed to parse RoleId as integer")
	}
	roleId := int(roleIdFloat)

	return models.Duty{
		PartitionKey:    dutyData["PartitionKey"].(string),
		RowKey:          rowKeyUUID,
		RoleId:          roleId,
		DutyName:        dutyData["DutyName"].(string),
		DutyDescription: dutyData["DutyDescription"].(string),
	}, nil
}
