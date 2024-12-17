package models

import (
	"time"
)

type Availability struct {
	ID         string    `json:"id" bson:"id"`
	EmployeeID string    `json:"employeeId" bson:"employeeId"`
	StartDate  time.Time `json:"startDate" bson:"startDate"`
	EndDate    time.Time `json:"endDate" bson:"endDate"`
	RoleIDs    []int     `json:"roleIds" bson:"roleIds"`
}
