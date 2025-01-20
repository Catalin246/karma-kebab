package mocks

import (
	"context"
	"time"

	"github.com/Catalin246/karma-kebab/models"

	"github.com/stretchr/testify/mock"
)

type MockEventService struct {
	mock.Mock
}

func (m *MockEventService) GetAll(ctx context.Context, start, end *time.Time) ([]models.Event, error) {
	args := m.Called(ctx, start, end)
	return args.Get(0).([]models.Event), args.Error(1)
}

func (m *MockEventService) GetByID(ctx context.Context, partitionKey, rowKey string) (*models.Event, error) {
	args := m.Called(ctx, partitionKey, rowKey)
	if args.Get(0) != nil {
		return args.Get(0).(*models.Event), args.Error(1)
	}
	return nil, args.Error(1)
}

func (m *MockEventService) Create(ctx context.Context, event models.Event) error {
	return m.Called(ctx, event).Error(0)
}

func (m *MockEventService) Update(ctx context.Context, partitionKey string, rowKey string, event models.Event) error {
	args := m.Called(ctx, partitionKey, rowKey, event)
	return args.Error(0)
}

func (m *MockEventService) Delete(ctx context.Context, partitionKey, rowKey string) error {
	return m.Called(ctx, partitionKey, rowKey).Error(0)
}

func (m *MockEventService) GetEventByShiftID(ctx context.Context, partitionKey string) (*models.Event, error) {
	args := m.Called(ctx, partitionKey)
	if args.Get(0) != nil {
		return args.Get(0).(*models.Event), args.Error(1)
	}
	return nil, args.Error(1)
}
