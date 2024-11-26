package services

import (
	"context"
	"event-service/models"
	"time"
)

type EventServiceInteface interface {
	// Create an event
	Create(ctx context.Context, event models.Event) error
	// Get an event by its ID
	GetByID(ctx context.Context, partitionKey, rowKey string) (*models.Event, error)
	// Get all events with optional date filtering
	GetAll(ctx context.Context, startDate, endDate *time.Time) ([]models.Event, error)
	// Update an event
	Update(ctx context.Context, partitionKey, rowKey string, event models.Event) error
	// Delete an event by its ID
	Delete(ctx context.Context, partitionKey, rowKey string) error
}
