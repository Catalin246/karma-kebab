package models

import "github.com/google/uuid"

// represents a duty assigned to a role
type Duty struct {
	PartitionKey    string    `json:"PartitionKey"`    // Azure Table Storage PartitionKey
	RowKey          uuid.UUID `json:"RowKey"`          // THIS IS ID OF THE TASK Rowkey - Primary Key (string representation of UUID)
	RoleId          uuid.UUID `json:"RoleId"`          // ID of the associated role (string representation of UUID)
	DutyName        string    `json:"DutyName"`        // Name of the duty
	DutyDescription string    `json:"DutyDescription"` // Detailed description
}
