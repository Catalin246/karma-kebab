package repositories

import (
	"context"
	"duty-service/models"
)

type InterfaceDutyRepository interface {
	GetAllDuties(ctx context.Context, filter string) ([]models.Duty, error)
	// GetDutyById(id uuid.UUID) (*models.Duty, error)
	// GetDutyByName(name string) (*models.Duty, error)
	// GetDutiesByRole(roleID uuid.UUID) ([]models.Duty, error)
	CreateDuty(ctx context.Context, duty models.Duty) error
	// UpdateDuty(duty *models.Duty) error
	DeleteDuty(ctx context.Context, partitionKey, rowKey string) error
}
