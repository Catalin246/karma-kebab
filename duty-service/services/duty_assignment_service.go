package services

import (
	"context"
	"duty-service/models"
	"duty-service/repositories"

	"github.com/google/uuid"
)

type DutyAssignmentService struct {
	repo repositories.InterfaceDutyAssignmentRepository
}

// NewDutyAssignmentService creates a new DutyAssignmentService
func NewDutyAssignmentService(repo repositories.InterfaceDutyAssignmentRepository) *DutyAssignmentService {
	return &DutyAssignmentService{repo: repo}
}

// GET all duties
func (s *DutyAssignmentService) GetAllDutyAssignmentsByShiftId(ctx context.Context, shiftId uuid.UUID) ([]models.DutyAssignment, error) {
	return s.repo.GetAllDutyAssignmentsByShiftId(ctx, shiftId)
}

// PUT update a duty
func (s *DutyAssignmentService) UpdateDutyAssignment(ctx context.Context, dutyAssignment models.DutyAssignment) error {
	return s.repo.UpdateDutyAssignment(ctx, dutyAssignment)
}

// DELETE a duty assignment by ShiftId and DutyId
func (s *DutyAssignmentService) DeleteDutyAssignment(ctx context.Context, shiftId uuid.UUID, dutyId string) error {
	return s.repo.DeleteDutyAssignment(ctx, shiftId, dutyId)
}
