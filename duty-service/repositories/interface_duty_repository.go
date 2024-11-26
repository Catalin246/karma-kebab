package repositories

import (
	"duty-service/models"

	"github.com/google/uuid"
)

type InterfaceDutyRepository interface {
	GetAllDuties() ([]models.Duty, error)
	GetDutyById(id uuid.UUID) (*models.Duty, error)
	GetDutyByName(name string) (*models.Duty, error)
	GetDutiesByRole(roleID uuid.UUID) ([]models.Duty, error)
	CreateDuty(duty *models.Duty) error
	UpdateDuty(duty *models.Duty) error
	DeleteDuty(id uuid.UUID) error
}
