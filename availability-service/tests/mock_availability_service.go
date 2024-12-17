package tests

import (
	"context"
	"time"

	"availability-service/handlers"
	"availability-service/models"

	"github.com/stretchr/testify/mock"
)

//MockAvailabilityService implements the interface
var _ handlers.IAvailability = (*MockAvailabilityService)(nil)

type MockAvailabilityService struct {
	mock.Mock
}

func (m *MockAvailabilityService) GetAll(ctx context.Context, startDate, endDate *time.Time, roleIDs []int) ([]models.Availability, error) {
	args := m.Called(ctx, startDate, endDate)
	return args.Get(0).([]models.Availability), args.Error(1)
}

func (m *MockAvailabilityService) GetByEmployeeID(ctx context.Context, employeeID string) ([]models.Availability, error) {
	args := m.Called(ctx, employeeID)
	return args.Get(0).([]models.Availability), args.Error(1)
}

func (m *MockAvailabilityService) Create(ctx context.Context, availability models.Availability) (*models.Availability, error) {
    args := m.Called(ctx, availability)
    if args.Get(0) == nil {
        return nil, args.Error(1)
    }
    return args.Get(0).(*models.Availability), args.Error(1)
}

func (m *MockAvailabilityService) Update(ctx context.Context, employeeID, id string, availability models.Availability) error {
	args := m.Called(ctx, employeeID, id, availability)
	return args.Error(0)
}

func (m *MockAvailabilityService) Delete(ctx context.Context, employeeID, id string) error {
	args := m.Called(ctx, employeeID, id)
	return args.Error(0)
}