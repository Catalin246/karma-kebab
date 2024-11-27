package services

import (
	"context"
	"duty-service/models"
	"duty-service/repositories"
)

type DutyService struct {
	repo repositories.InterfaceDutyRepository
}

// NewDutyService creates a new DutyService
func NewDutyService(repo repositories.InterfaceDutyRepository) *DutyService {
	return &DutyService{repo: repo}
}

// GET all duties
func (s *DutyService) GetAllDuties(ctx context.Context, name string) ([]models.Duty, error) {
	var filter string

	if name != "" { // Check if the name is not empty
		filter = "Name eq '" + name + "'" // Construct the filter for the name
	}

	return s.repo.GetAllDuties(ctx, filter)
}

// GET duty by id
func (s *DutyService) GetDutyById(ctx context.Context, partitionKey, rowKey string) (*models.Duty, error) {
	return s.repo.GetDutyById(ctx, partitionKey, rowKey)
}

// POST create duty
func (s *DutyService) CreateDuty(ctx context.Context, duty models.Duty) error {
	return s.repo.CreateDuty(ctx, duty)
}

// DELETE delete a duty
func (s *DutyService) DeleteDuty(ctx context.Context, partitionKey, rowKey string) error {
	return s.repo.DeleteDuty(ctx, partitionKey, rowKey)
}
