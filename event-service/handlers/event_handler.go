package handlers

import (
	"encoding/json"
	"net/http"
)

// GetEvents retrievs all events
func GetEvents(w http.ResponseWriter, r *http.Request) {
	// TO DO
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "GetEvents"})
}

// GetEventByID retrieves a specific event
func GetEventByID(w http.ResponseWriter, r *http.Request) {
	// TO DO
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "GetEventByID"})
}

// CreateEvent creates a new event
func CreateEvent(w http.ResponseWriter, r *http.Request) {
	// TO DO
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "CreateEvent"})
}

// UpdateEvent updates an existing event
func UpdateEvent(w http.ResponseWriter, r *http.Request) {
	// TO DO
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "UpdateEvent"})
}

// DeleteEvent deletes an existing event
func DeleteEvent(w http.ResponseWriter, r *http.Request) {
	// TO DO
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "DeleteEvent"})
}
