package services

import (
	"context"
	"duty-service/models"
)

type InterfaceDutyService interface {
	GetAllDuties(ctx context.Context, name string) ([]models.Duty, error)
	//the rest
}
