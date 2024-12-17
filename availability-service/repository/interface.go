package repository

import (
	"availability-service/models"
	"context"
	"time"
)

type AvailabilityRepository interface {
    Create(ctx context.Context, availability models.Availability) error
    Update(ctx context.Context, employeeID string, availability models.Availability) error
    Delete(ctx context.Context, employeeID string, id string) error
    GetByEmployeeID(ctx context.Context, employeeID string) ([]models.Availability, error)
    GetAll(ctx context.Context, startDate, endDate *time.Time, roleIDs []int) ([]models.Availability, error)
    GetOverlappingAvailabilities(ctx context.Context, availability models.Availability) ([]models.Availability, error)
}
