package models

import "github.com/google/uuid"

// A duty assigned to a Shift (unique for each employee and for each shift)
type DutyAssignment struct {
	PartitionKey           uuid.UUID            `json:"PartitionKey"`           // ShiftID (used as PartitionKey in Azure Table Storage)
	RowKey                 uuid.UUID            `json:"RowKey"`                 // DutyID (used as RowKey in Azure Table Storage)
	DutyAssignmentStatus   DutyAssignmentStatus `json:"DutyAssignmentStatus"`   // DutyAssignmentStatus (e.g., "Completed", "Incomplete")
	DutyAssignmentImageUrl *string              `json:"DutyAssignmentImageUrl"` // URL to an image (optional, nullable)
	DutyAssignmentNote     *string              `json:"DutyAssignmentNote"`     // Additional note (optional, nullable)
}

////////////////////////////////////////

// ENUM for DutyAssignment Status
type DutyAssignmentStatus string

// List of possible duty assignment statuses
const (
	StatusCompleted  DutyAssignmentStatus = "Completed"
	StatusIncomplete DutyAssignmentStatus = "Incomplete"
)

// contains all valid status values
var ValidDutyAssignmentStatuses = map[DutyAssignmentStatus]struct{}{
	StatusCompleted:  {},
	StatusIncomplete: {},
}

// checks if the status is valid
func ValidateDutyAssignmentStatus(status DutyAssignmentStatus) bool {
	_, valid := ValidDutyAssignmentStatuses[status]
	return valid
}
