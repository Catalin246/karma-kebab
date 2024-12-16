package services

import (
	"context"
	"duty-service/models"

	"github.com/google/uuid"
)

type InterfaceDutyService interface {
	GetAllDuties(ctx context.Context, name string) ([]models.Duty, error)
	GetDutyById(ctx context.Context, partitionKey, rowKey string) (*models.Duty, error)
	GetDutiesByRole(ctx context.Context, roleId uuid.UUID) ([]models.Duty, error)
	CreateDuty(ctx context.Context, duty models.Duty) error
	UpdateDuty(ctx context.Context, partitionKey, rowKey string, duty models.Duty) error
	DeleteDuty(ctx context.Context, partitionKey, rowKey string) error
}
