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

		// Unmarshal each entity and add duties[]
		for _, entity := range page.Entities {
			var dutyData map[string]interface{}

			if err := json.Unmarshal(entity, &dutyData); err != nil {
				return nil, fmt.Errorf("failed to unmarshal duty: %v", err)
			}

			//TODO BETH: put these parsing to another method 26/11
			// Parse RowKey as UUID
			rowKeyUUID, err := uuid.Parse(dutyData["RowKey"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse Duty RowKey as UUID: %v", err)
			}

			// Parse RoleId as UUID
			roleIdUUID, err := uuid.Parse(dutyData["RoleId"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse Duty RoleId as UUID: %v", err)
			}

			duty := models.Duty{
				PartitionKey:    dutyData["PartitionKey"].(string),
				RowKey:          rowKeyUUID,
				RoleId:          roleIdUUID,
				DutyName:        dutyData["DutyName"].(string),
				DutyDescription: dutyData["DutyDescription"].(string),
			}

			duties = append(duties, duty)
		}
	}
	return duties, nil
}
