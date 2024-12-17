package repositories

import (
	"context"
	"duty-service/models"

	"github.com/google/uuid"
)

type InterfaceDutyRepository interface {
	GetAllDuties(ctx context.Context, filter string) ([]models.Duty, error)
	GetDutyById(ctx context.Context, partitionKey, rowKey string) (*models.Duty, error)
	// GetDutyByName(name string) (*models.Duty, error)
	GetDutiesByRole(ctx context.Context, roleId uuid.UUID) ([]models.Duty, error)
	CreateDuty(ctx context.Context, duty models.Duty) error
	UpdateDuty(ctx context.Context, partitionKey, rowKey string, duty models.Duty) error
	DeleteDuty(ctx context.Context, partitionKey, rowKey string) error
}