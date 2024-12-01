package services

import (
	"context"
	"duty-service/models"
	"duty-service/repositories"
	"fmt"

	"github.com/google/uuid"
)

type DutyAssignmentService struct {
	repo     repositories.InterfaceDutyAssignmentRepository
	dutyRepo repositories.InterfaceDutyRepository
}

// NewDutyAssignmentService creates a new DutyAssignmentService
func NewDutyAssignmentService(repo repositories.InterfaceDutyAssignmentRepository, dutyRepo repositories.InterfaceDutyRepository) *DutyAssignmentService {
	return &DutyAssignmentService{
		repo:     repo,
		dutyRepo: dutyRepo,
	}
}

// GET all duties
func (s *DutyAssignmentService) GetAllDutyAssignmentsByShiftId(ctx context.Context, shiftId uuid.UUID) ([]models.DutyAssignment, error) {
	return s.repo.GetAllDutyAssignmentsByShiftId(ctx, shiftId)
}

// POST create duty assignments for a given ShiftId and RoleId
func (s *DutyAssignmentService) CreateDutyAssignments(ctx context.Context, shiftId uuid.UUID, roleId uuid.UUID) error {
	// Fetch all duties by RoleId
	duties, err := s.dutyRepo.GetDutiesByRole(ctx, roleId)
	if err != nil {
		return fmt.Errorf("failed to fetch duties for RoleId %s: %v", roleId, err)
	}

	// Create duty assignments for the ShiftId
	return s.repo.CreateDutyAssignments(ctx, shiftId, duties)
}

// PUT update a duty
func (s *DutyAssignmentService) UpdateDutyAssignment(ctx context.Context, dutyAssignment models.DutyAssignment) error {
	return s.repo.UpdateDutyAssignment(ctx, dutyAssignment)
}

// DELETE a duty assignment by ShiftId and DutyId
func (s *DutyAssignmentService) DeleteDutyAssignment(ctx context.Context, shiftId uuid.UUID, dutyId string) error {
	return s.repo.DeleteDutyAssignment(ctx, shiftId, dutyId)
}
