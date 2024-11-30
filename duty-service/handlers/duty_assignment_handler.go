package handlers

import (
	"context"
	"duty-service/services"
	"encoding/json"
	"net/http"

	"github.com/google/uuid"
)

type DutyAssignmentHandler struct {
	service services.InterfaceDutyAssignmentService
}

// NewDutyAssignmentHandler creates a new DutyAssignmentHandler
func NewDutyAssignmentHandler(service services.InterfaceDutyAssignmentService) *DutyAssignmentHandler {
	return &DutyAssignmentHandler{service: service}
}

func (h *DutyAssignmentHandler) GetAllDutyAssignmentsByShiftId(w http.ResponseWriter, r *http.Request) {

	// Parse the ShiftId from the query parameters
	query := r.URL.Query()
	shiftIdStr := query.Get("shiftId") // Get the shiftId parameter from the query string

	// Ensure that the shiftId parameter is provided and is valid
	if shiftIdStr == "" {
		http.Error(w, "Missing 'shiftId' query parameter", http.StatusBadRequest)
		return
	}

	// Parse the shiftId from string to uuid.UUID
	shiftIdUUID, err := uuid.Parse(shiftIdStr)
	if err != nil {
		http.Error(w, "Invalid 'shiftId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Call the service method to fetch duty assignments by shiftId
	dutyAssignments, err := h.service.GetAllDutyAssignmentsByShiftId(context.Background(), shiftIdUUID)
	if err != nil {
		http.Error(w, "Failed to retrieve duty assignments: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Set the response content type to JSON
	w.Header().Set("Content-Type", "application/json")

	// Encode the duty assignments as JSON and send the response
	err = json.NewEncoder(w).Encode(dutyAssignments)
	if err != nil {
		http.Error(w, "Failed to encode response: "+err.Error(), http.StatusInternalServerError)
	}
}
