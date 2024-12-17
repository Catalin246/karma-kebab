package repositories

import (
	"context"
	"duty-service/models"
	"io"

	"github.com/google/uuid"
)

type InterfaceDutyAssignmentRepository interface {
	GetAllDutyAssignmentsByShiftId(ctx context.Context, shiftId uuid.UUID) ([]models.DutyAssignment, error)
	CreateDutyAssignments(ctx context.Context, shiftId uuid.UUID, duties []models.Duty) error
	UpdateDutyAssignment(ctx context.Context, dutyAssignment models.DutyAssignment, image io.Reader) error
	DeleteDutyAssignment(ctx context.Context, shiftId uuid.UUID, dutyId uuid.UUID) error
}