package models

type EventShift struct {
	PartitionKey string `json:"PartitionKey"` // EventID
	RowKey       string `json:"RowKey"`       // ShiftID
}
