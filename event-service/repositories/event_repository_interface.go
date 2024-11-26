package repositories

import (
	"context"
	"event-service/models"
)

type EventRepositoryInterface interface {
	// Create inserts a new event into the database
	Create(ctx context.Context, event models.Event) error
	// GetByID retrieves an event by PartitionKey and RowKey
	GetByID(ctx context.Context, partitionKey, rowKey string) (*models.Event, error)
	// GetAll retrieves all events, optionally filtered by date range
	GetAll(ctx context.Context, filter string) ([]models.Event, error)
	// Update modifies an existing event
	Update(ctx context.Context, partitionKey, rowKey string, event models.Event) error
	// Delete removes an event by PartitionKey and RowKey
	Delete(ctx context.Context, partitionKey, rowKey string) error
}
