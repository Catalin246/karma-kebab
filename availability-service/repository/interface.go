// repository/interface.go
package repository

import (
    "context"
    "availability-service/models"
)

type AvailabilityRepository interface {
    GetAll(ctx context.Context) ([]models.Availability, error)
    GetByID(ctx context.Context, id string) (*models.Availability, error)
    Create(ctx context.Context, availability models.Availability) error
    Update(ctx context.Context, id string, availability models.Availability) error
    Delete(ctx context.Context, id string) error
}