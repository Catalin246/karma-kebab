package repository

import (
	"availability-service/models"
	"context"
	"encoding/json"
	"fmt"

	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
)

type CosmosAvailabilityRepository struct {
    container *azcosmos.ContainerClient
}

func NewCosmosAvailabilityRepository(container *azcosmos.ContainerClient) *CosmosAvailabilityRepository {
    return &CosmosAvailabilityRepository{
        container: container,
    }
}

// Make sure this implements the interface
var _ AvailabilityRepository = (*CosmosAvailabilityRepository)(nil)

// func (r *CosmosAvailabilityRepository) GetAll(ctx context.Context) ([]models.Availability, error) {
//     query := "SELECT * FROM c"
//     queryPager := r.container.NewQueryItemsPager(query, azcosmos.QueryOptions{})

//     var availabilities []models.Availability
//     for queryPager.More() {
//         queryResponse, err := queryPager.NextPage(ctx)
//         if err != nil {
//             return nil, fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
//         }

//         for _, item := range queryResponse.Items {
//             var availability models.Availability
//             if err := json.Unmarshal(item, &availability); err != nil {
//                 return nil, fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
//             }
//             availabilities = append(availabilities, availability)
//         }
//     }

//     return availabilities, nil
// }
func (r *CosmosAvailabilityRepository) GetAll(ctx context.Context, employeeID string) ([]models.Availability, error) {
    query := "SELECT * FROM c"
    partitionKey := azcosmos.NewPartitionKeyString(employeeID) // Use EmployeeID as partition key

    queryPager := r.container.NewQueryItemsPager(query, partitionKey, &azcosmos.QueryOptions{})

    var availabilities []models.Availability
    for queryPager.More() {
        queryResponse, err := queryPager.NextPage(ctx)
        if err != nil {
            return nil, fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
        }

        for _, item := range queryResponse.Items {
            var availability models.Availability
            if err := json.Unmarshal(item, &availability); err != nil {
                return nil, fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
            }
            availabilities = append(availabilities, availability)
        }
    }

    return availabilities, nil
}


// func (r *CosmosAvailabilityRepository) GetByID(ctx context.Context, id string) (*models.Availability, error) {
// 	pk := azcosmos.NewPartitionKeyString(id)
// 	response, err := r.container.ReadItem(ctx, pk, id, nil)
// 	if err != nil {
// 		if isNotFoundError(err) {
// 			return nil, models.ErrNotFound
// 		}
// 		return nil, fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
// 	}

// 	var availability models.Availability
// 	if err := json.Unmarshal(response.Value, &availability); err != nil {
// 		return nil, fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
// 	}

// 	return &availability, nil
// }
func (r *CosmosAvailabilityRepository) GetByID(ctx context.Context, employeeID, id string) (*models.Availability, error) {
    partitionKey := azcosmos.NewPartitionKeyString(employeeID)
    response, err := r.container.ReadItem(ctx, partitionKey, id, nil)
    if err != nil {
        if isNotFoundError(err) {
            return nil, models.ErrNotFound
        }
        return nil, fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
    }

    var availability models.Availability
    if err := json.Unmarshal(response.Value, &availability); err != nil {
        return nil, fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
    }

    return &availability, nil
}


func (r *CosmosAvailabilityRepository) Create(ctx context.Context, availability models.Availability) error {
    partitionKey := azcosmos.NewPartitionKeyString(availability.EmployeeID)

    data, err := json.Marshal(availability)
    if err != nil {
        return fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
    }

    _, err = r.container.CreateItem(ctx, partitionKey, data, nil)
    if err != nil {
        if isConflictError(err) {
            return models.ErrConflict
        }
        return fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
    }

    return nil
}


// func (r *CosmosAvailabilityRepository) Update(ctx context.Context, id string, availability models.Availability) error {
// 	pk := azcosmos.NewPartitionKeyString(id)

// 	data, err := json.Marshal(availability)
// 	if err != nil {
// 		return fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
// 	}

// 	_, err = r.container.ReplaceItem(ctx, pk, id, data, nil)
// 	if err != nil {
// 		if isNotFoundError(err) {
// 			return models.ErrNotFound
// 		}
// 		return fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
// 	}

// 	return nil
// }
func (r *CosmosAvailabilityRepository) Update(ctx context.Context, employeeID string, availability models.Availability) error {
    partitionKey := azcosmos.NewPartitionKeyString(employeeID)

    data, err := json.Marshal(availability)
    if err != nil {
        return fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
    }

    _, err = r.container.ReplaceItem(ctx, partitionKey, availability.ID, data, nil)
    if err != nil {
        if isNotFoundError(err) {
            return models.ErrNotFound
        }
        return fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
    }

    return nil
}


func (r *CosmosAvailabilityRepository) Delete(ctx context.Context, employeeID, id string) error {
    partitionKey := azcosmos.NewPartitionKeyString(employeeID)

    _, err := r.container.DeleteItem(ctx, partitionKey, id, nil)
    if err != nil {
        if isNotFoundError(err) {
            return models.ErrNotFound
        }
        return fmt.Errorf("%w: %v", models.ErrDatabaseOperation, err)
    }

    return nil
}

func isNotFoundError(err error) bool {
	// Add specific Cosmos DB not found error check
	return err.Error() == "NotFound" // You might need to adjust this based on actual Cosmos DB error types
}

func isConflictError(err error) bool {
	// Add specific Cosmos DB conflict error check
	return err.Error() == "Conflict" // You might need to adjust this based on actual Cosmos DB error types
}
