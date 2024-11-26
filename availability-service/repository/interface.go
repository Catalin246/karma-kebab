package repository

import (
	"availability-service/models"
	"context"
	"time"
)

type AvailabilityRepository interface {
    GetAll(ctx context.Context, startDate, endDate *time.Time) ([]models.Availability, error) //allows for filtering by date
    GetByEmployeeID(ctx context.Context, employeeID string) ([]models.Availability, error)
    Create(ctx context.Context, availability models.Availability) error
    Update(ctx context.Context, employeeID string, availability models.Availability) error
    Delete(ctx context.Context, employeeID string, id string) error
}

