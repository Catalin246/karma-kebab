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
	StatusCompletes Status = "Completes"
	StatusCancelled Status = "Cancelled"
)

// Event class
type Event struct {
	PartitionKey string    `json:"partitionKey"` // Azure Table Storage PartitionKey
	RowKey       string    `json:"rowKey"`       // Azure Table Storage RowKey
	ID           uuid.UUID `json:"id"`           // UUID for the event (used internally)
	Date         time.Time `json:"date"`         // Event date and time
	Address      string    `json:"address"`      // Event address
	Venue        string    `json:"venue"`        // Venue name
	Description  string    `json:"description"`  // Description of the event
	Money        float64   `json:"money"`        // Associated cost/money
	Status       Status    `json:"status"`       // Event status
	Person       Person    `json:"person"`       // Associated person
	Note         string    `json:"note"`         // Additional notes
}
