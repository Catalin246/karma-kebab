package models

import (
	"time"

	"github.com/google/uuid"
)

// Enum for Event Status
type Status string

const (
	StatusPlanned   Status = "Planned"
	StatusOngoing   Status = "Ongoing"
	StatusCompleted Status = "Completed"
	StatusCancelled Status = "Cancelled"
)

// Event class
type Event struct {
	PartitionKey string      `json:"partitionKey"` // Azure Table Storage PartitionKey
	RowKey       uuid.UUID   `json:"rowKey"`       // Azure Table Storage RowKey
	StartTime    time.Time   `json:"startTime"`    // Event date and time
	EndTime      time.Time   `json:"endTime"`      // Event date and time
	Address      string      `json:"address"`      // Event address
	Venue        string      `json:"venue"`        // Venue name
	Description  string      `json:"description"`  // Description of the event
	Money        float64     `json:"money"`        // Associated cost/money
	Status       Status      `json:"status"`       // Event status
	Person       Person      `json:"person"`       // Associated person
	Note         string      `json:"note"`         // Additional notes
	ShiftIDs     []uuid.UUID `json:"shiftIDs"`     // List of shift IDs (UUIDs)
	RoleIDs      []int       `json:"roleIDs"`      // List of role IDs (ints)
}
