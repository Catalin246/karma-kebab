package models

// Duty represents a duty assigned to a role
type Duty struct {
	Id              int    `json:"id"`          // Primary Key
	RoleId          int    `json:"role_id"`     // ID of the associated role
	DutyName        string `json:"name"`        // Name of the duty
	DutyDescription string `json:"description"` // Detailed description
}
