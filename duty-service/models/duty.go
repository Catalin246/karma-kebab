package models

// Duty represents a duty assigned to a role
type Duty struct {
	Id              string `json:"id"`          // Primary Key (string representation of UUID)
	RoleId          string `json:"role_id"`     // ID of the associated role (string representation of UUID)
	DutyName        string `json:"name"`        // Name of the duty
	DutyDescription string `json:"description"` // Detailed description
}
