package service

import (
	"context"
	"time"

	"github.com/google/uuid"
	"availability-service/models"
	"availability-service/repository"
)

type AvailabilityService struct {
	repo repository.AvailabilityRepository
}

func NewAvailabilityService(repo repository.AvailabilityRepository) *AvailabilityService {
	return &AvailabilityService{
		repo: repo,
	}
}

func (s *AvailabilityService) GetAll(ctx context.Context) ([]models.Availability, error) {
	return s.repo.GetAll(ctx)
}

func (s *AvailabilityService) GetByID(ctx context.Context, id string) (*models.Availability, error) {
	if id == "" {
		return nil, models.ErrInvalidID
	}
	return s.repo.GetByID(ctx, id)
}

func (s *AvailabilityService) Create(ctx context.Context, availability models.Availability) (*models.Availability, error) {
	if err := s.validateAvailability(availability); err != nil {
		return nil, err
	}

	if availability.ID == "" {
		availability.ID = uuid.New().String()
	}

	err := s.repo.Create(ctx, availability)
	if err != nil {
		return nil, err
	}

	return &availability, nil
}

func (s *AvailabilityService) Update(ctx context.Context, id string, availability models.Availability) error {
	if id == "" {
		return models.ErrInvalidID
	}

	if err := s.validateAvailability(availability); err != nil {
		return err
	}

	availability.ID = id

	return s.repo.Update(ctx, id, availability)
}

func (s *AvailabilityService) Delete(ctx context.Context, id string) error {
	if id == "" {
		return models.ErrInvalidID
	}
	return s.repo.Delete(ctx, id)
}

func (s *AvailabilityService) validateAvailability(availability models.Availability) error {
	if availability.EmployeeID == "" {
		return models.ErrInvalidAvailability
	}

	if availability.StartDate.IsZero() || availability.EndDate.IsZero() {
		return models.ErrInvalidAvailability
	}

	if availability.EndDate.Before(availability.StartDate) {
		return models.ErrInvalidAvailability
	}

	if availability.StartDate.Before(time.Now()) {
		return models.ErrInvalidAvailability
	}

	return nil
}