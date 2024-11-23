package services

import (
	"duty-service/models"
	"duty-service/repositories"

	"github.com/google/uuid"
)

type DutyService interface {
	GetAllDuties() ([]models.Duty, error)
	GetDutyById(id uuid.UUID) (*models.Duty, error)
	GetDutiesByRole(roleId uuid.UUID) ([]models.Duty, error)
	CreateDuty(duty *models.Duty) error
	EditDuty(duty *models.Duty) error
	DeleteDuty(id uuid.UUID) error
}

type dutyServiceImpl struct {
	repository repositories.DutyRepository
}

// NewDutyService creates a new instance of the DutyService
func NewDutyService(repo repositories.DutyRepository) DutyService {
	return &dutyServiceImpl{repository: repo}
}

func (s *dutyServiceImpl) GetAllDuties() ([]models.Duty, error) {
	return s.repository.GetAllDuties()
}

func (s *dutyServiceImpl) GetDutyById(id uuid.UUID) (*models.Duty, error) {
	return s.repository.GetDutyById(id)
}

func (s *dutyServiceImpl) GetDutiesByRole(roleId uuid.UUID) ([]models.Duty, error) {
	return s.repository.GetDutiesByRole(roleId)
}

func (s *dutyServiceImpl) CreateDuty(duty *models.Duty) error {
	return s.repository.CreateDuty(duty)
}

func (s *dutyServiceImpl) EditDuty(duty *models.Duty) error {
	return s.repository.UpdateDuty(duty)
}

func (s *dutyServiceImpl) DeleteDuty(id uuid.UUID) error {
	return s.repository.DeleteDuty(id)
}
