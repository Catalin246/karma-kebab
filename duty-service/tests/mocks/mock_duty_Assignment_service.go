package mocks

import (
	"context"
	"duty-service/models"

	"github.com/google/uuid"
	"github.com/stretchr/testify/mock"
)

type MockDutyAssignmentService struct {
	mock.Mock
}

func (m *MockDutyAssignmentService) GetAllDutyAssignmentsByShiftId(ctx context.Context, shiftId uuid.UUID) ([]models.DutyAssignment, error) {
	args := m.Called(ctx, shiftId)
	return args.Get(0).([]models.DutyAssignment), args.Error(1)
}

func (m *MockDutyAssignmentService) CreateDutyAssignments(ctx context.Context, shiftId uuid.UUID, roleId uuid.UUID) error {
	args := m.Called(ctx, shiftId, roleId)
	return args.Error(0)
}

func (m *MockDutyAssignmentService) UpdateDutyAssignment(ctx context.Context, dutyAssignment models.DutyAssignment) error {
	args := m.Called(ctx, dutyAssignment)
	return args.Error(0)
}

func (m *MockDutyAssignmentService) DeleteDutyAssignment(ctx context.Context, shiftId uuid.UUID, dutyId uuid.UUID) error {
	args := m.Called(ctx, shiftId, dutyId)
	return args.Error(0)
}
