package models

import "github.com/google/uuid"

// Duty represents a duty assigned to a role
type Duty struct {
	PartitionKey    string    `json:"partitionKey"` // Azure Table Storage PartitionKey
	RowKey          uuid.UUID `json:"rowKey"`       // THIS IS ID OF THE TASK Rowkey - Primary Key (string representation of UUID)
	RoleId          uuid.UUID `json:"role_id"`      // ID of the associated role (string representation of UUID)
	DutyName        string    `json:"name"`         // Name of the duty
	DutyDescription string    `json:"description"`  // Detailed description
}
