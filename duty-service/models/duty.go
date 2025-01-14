package models

import (
	"time"

	"github.com/google/uuid"
)

// represents a duty assigned to a role
type Duty struct {
	PartitionKey    string    `json:"PartitionKey"`    // Azure Table Storage PartitionKey
	RowKey          uuid.UUID `json:"RowKey"`          // THIS IS ID OF THE TASK Rowkey - Primary Key (string representation of UUID)
	RoleId          int       `json:"RoleId"`          // ID of the associated role (now an int because it is an enum in Employee ms)
	DutyName        string    `json:"DutyName"`        // Name of the duty
	DutyDescription string    `json:"DutyDescription"` // Detailed description
}
type ClockInMessage struct {
	ShiftID     uuid.UUID `json:"shift_id"`
	ClockInTime time.Time `json:"clock_in_time"`
	//RoleId      uuid.UUID `json:"role_id"` //or roleID?? - //Beth: changed this to int but commented out the original for Myrthe (delete this comment later)
	RoleId int `json:"role_id"`
}
