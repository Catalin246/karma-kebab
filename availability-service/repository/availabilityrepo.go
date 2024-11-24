package repository

import (
    "context"
    "encoding/json"
    "fmt"
    "availability-service/models"
	"availability-service/models/error.go"
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
var _ repository.AvailabilityRepository = (*CosmosAvailabilityRepository)(nil)

func (r *CosmosAvailabilityRepository) GetAll(ctx context.Context) ([]models.Availability, error) {
    query := "SELECT * FROM c"
    queryPager := r.container.NewQueryItemsPager(query, azcosmos.QueryOptions{})

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

func (r *CosmosAvailabilityRepository) GetByID(ctx context.Context, id string) (*models.Availability, error) {
	pk := azcosmos.NewPartitionKeyString(id)
	response, err := r.container.ReadItem(ctx, pk, id, nil)
	if err != nil {
		if isNotFoundError(err) {
			return nil, errors.ErrNotFound
		}
		return nil, fmt.Errorf("%w: %v", errors.ErrDatabaseOperation, err)
	}

	var availability models.Availability
	if err := json.Unmarshal(response.Value, &availability); err != nil {
		return nil, fmt.Errorf("%w: %v", errors.ErrDatabaseOperation, err)
	}

	return &availability, nil
}

func (r *CosmosAvailabilityRepository) Create(ctx context.Context, availability models.Availability) error {
	pk := azcosmos.NewPartitionKeyString(availability.ID)

	data, err := json.Marshal(availability)
	if err != nil {
		return fmt.Errorf("%w: %v", errors.ErrDatabaseOperation, err)
	}

	_, err = r.container.CreateItem(ctx, pk, data, nil)
	if err != nil {
		if isConflictError(err) {
			return errors.ErrConflict
		}
		return fmt.Errorf("%w: %v", errors.ErrDatabaseOperation, err)
	}

	return nil
}

func (r *CosmosAvailabilityRepository) Update(ctx context.Context, id string, availability models.Availability) error {
	pk := azcosmos.NewPartitionKeyString(id)

	data, err := json.Marshal(availability)
	if err != nil {
		return fmt.Errorf("%w: %v", errors.ErrDatabaseOperation, err)
	}

	_, err = r.container.ReplaceItem(ctx, pk, id, data, nil)
	if err != nil {
		if isNotFoundError(err) {
			return errors.ErrNotFound
		}
		return fmt.Errorf("%w: %v", errors.ErrDatabaseOperation, err)
	}

	return nil
}

func (r *CosmosAvailabilityRepository) Delete(ctx context.Context, id string) error {
	pk := azcosmos.NewPartitionKeyString(id)

	_, err := r.container.DeleteItem(ctx, pk, id, nil)
	if err != nil {
		if isNotFoundError(err) {
			return errors.ErrNotFound
		}
		return fmt.Errorf("%w: %v", errors.ErrDatabaseOperation, err)
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
