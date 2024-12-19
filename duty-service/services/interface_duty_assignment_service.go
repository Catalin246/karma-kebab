package services

import (
	"context"
	"duty-service/models"
	"mime/multipart"

	"github.com/google/uuid"
)

type InterfaceDutyAssignmentService interface {
	GetAllDutyAssignmentsByShiftId(ctx context.Context, shiftId uuid.UUID) ([]models.DutyAssignment, error)
	CreateDutyAssignments(ctx context.Context, shiftId uuid.UUID, roleId int) error
	UpdateDutyAssignment(ctx context.Context, dutyAssignment models.DutyAssignment, file multipart.File) error
	DeleteDutyAssignment(ctx context.Context, shiftId uuid.UUID, dutyId uuid.UUID) error
}
