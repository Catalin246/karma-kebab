package services

import (
	"context"
	"duty-service/models"

	"github.com/google/uuid"
)

type InterfaceDutyAssignmentService interface {
	GetAllDutyAssignmentsByShiftId(ctx context.Context, shiftId uuid.UUID) ([]models.DutyAssignment, error)
	CreateDutyAssignments(ctx context.Context, shiftId uuid.UUID, roleId uuid.UUID) error
	UpdateDutyAssignment(ctx context.Context, dutyAssignment models.DutyAssignment) error
	DeleteDutyAssignment(ctx context.Context, shiftId uuid.UUID, dutyId string) error
}
