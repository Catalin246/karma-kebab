package service

import (
	"context"
	"log"
	"time"

	"availability-service-2/models"
	"availability-service-2/repository"

	"github.com/google/uuid"
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
func (s *AvailabilityService) GetAll(ctx context.Context, startDate, endDate *time.Time) ([]models.Availability, error) {

	return s.repo.GetAll(ctx, startDate, endDate)
}

// Fetch a specific availability record by EmployeeID
func (s *AvailabilityService) GetByEmployeeID(ctx context.Context, employeeID string) ([]models.Availability, error) {
	if employeeID == "" {
		return nil, models.ErrInvalidID
	}
	return s.repo.GetByEmployeeID(ctx, employeeID)
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
    // Log the availability being validated
    log.Printf("Validating availability: %+v", availability)

    if availability.EmployeeID == "" {
        log.Println("Employee ID is empty")
        return models.ErrInvalidAvailability
    }

    if availability.StartDate.IsZero() || availability.EndDate.IsZero() {
        log.Println("Start date or end date is zero")
        return models.ErrInvalidAvailability
    }

    if availability.EndDate.Before(availability.StartDate) {
        log.Println("End date is before start date")
        return models.ErrInvalidAvailability
    }

    if availability.StartDate.Before(time.Now()) {
        log.Println("Start date is in the past")
        return models.ErrInvalidAvailability
    }

    log.Println("Availability validation successful")
    return nil
}
