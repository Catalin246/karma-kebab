package mocks

import (
	"context"
	"duty-service/models"

	"github.com/google/uuid"
	"github.com/stretchr/testify/mock"
)

// MockDutyService is a full mock implementation of InterfaceDutyService
type MockDutyService struct {
	mock.Mock
}

func (m *MockDutyService) GetAllDuties(ctx context.Context, name string) ([]models.Duty, error) {
	args := m.Called(ctx, name)
	return args.Get(0).([]models.Duty), args.Error(1)
}

func (m *MockDutyService) GetDutyById(ctx context.Context, partitionKey, rowKey string) (*models.Duty, error) {
	args := m.Called(ctx, partitionKey, rowKey)
	return args.Get(0).(*models.Duty), args.Error(1)
}

func (m *MockDutyService) GetDutiesByRole(ctx context.Context, roleId uuid.UUID) ([]models.Duty, error) {
	args := m.Called(ctx, roleId)
	return args.Get(0).([]models.Duty), args.Error(1)
}

func (m *MockDutyService) CreateDuty(ctx context.Context, duty models.Duty) error {
	args := m.Called(ctx, duty)
	return args.Error(0)
}

func (m *MockDutyService) UpdateDuty(ctx context.Context, partitionKey, rowKey string, duty models.Duty) error {
	args := m.Called(ctx, partitionKey, rowKey, duty)
	return args.Error(0)
}

func (m *MockDutyService) DeleteDuty(ctx context.Context, partitionKey, rowKey string) error {
	args := m.Called(ctx, partitionKey, rowKey)
	return args.Error(0)
}
