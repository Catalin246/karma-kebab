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

// Fetch all availability records for a specific EmployeeID with optional date range filter
func (s *AvailabilityService) GetAll(ctx context.Context, employeeID string, startDate, endDate *time.Time) ([]models.Availability, error) {
    if employeeID == "" {
        return nil, models.ErrInvalidAvailability
    }
    
    return s.repo.GetAll(ctx, employeeID, startDate, endDate)
}

// Fetch a specific availability record by ID and EmployeeID
func (s *AvailabilityService) GetByID(ctx context.Context, employeeID, id string) (*models.Availability, error) {
	if employeeID == "" || id == "" {
		return nil, models.ErrInvalidID
	}
	return s.repo.GetByID(ctx, employeeID, id)
}

// Create a new availability record
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

// Update an existing availability record by ID and EmployeeID
func (s *AvailabilityService) Update(ctx context.Context, employeeID, id string, availability models.Availability) error {
	if employeeID == "" || id == "" {
		return models.ErrInvalidID
	}

	if err := s.validateAvailability(availability); err != nil {
		return err
	}

	availability.ID = id
	availability.EmployeeID = employeeID

	return s.repo.Update(ctx, employeeID, availability)
}

// Delete an availability record by ID and EmployeeID
func (s *AvailabilityService) Delete(ctx context.Context, employeeID, id string) error {
	if employeeID == "" || id == "" {
		return models.ErrInvalidID
	}
	return s.repo.Delete(ctx, employeeID, id)
}

// Validate the availability record's fields
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
