package services

import (
	"context"
	"duty-service/models"
)

type InterfaceDutyService interface {
	GetAllDuties(ctx context.Context, name string) ([]models.Duty, error)

	CreateDuty(ctx context.Context, duty models.Duty) error
	DeleteDuty(ctx context.Context, partitionKey, rowKey string) error
}
