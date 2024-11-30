package models

// A duty assigned to a Shift (unique for each employee and for each shift)
type DutyAssignment struct {
	PartitionKey           string               `json:"PartitionKey"`           // ShiftID (used as PartitionKey in Azure Table Storage)
	RowKey                 string               `json:"RowKey"`                 // DutyID (used as RowKey in Azure Table Storage)
	DutyAssignmentStatus   DutyAssignmentStatus `json:"DutyAssignmentStatus"`   // DutyAssignmentStatus (e.g., "completed", "incompleted")
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
