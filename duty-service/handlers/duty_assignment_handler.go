package handlers

import (
	"context"
	"duty-service/models"
	"duty-service/services"
	"encoding/json"
	"net/http"

	"github.com/google/uuid"
	"github.com/gorilla/mux"
)

type DutyAssignmentHandler struct {
	service services.InterfaceDutyAssignmentService
}

// NewDutyAssignmentHandler creates a new DutyAssignmentHandler
func NewDutyAssignmentHandler(service services.InterfaceDutyAssignmentService) *DutyAssignmentHandler {
	return &DutyAssignmentHandler{service: service}
}

// GetAllDutyAssignmentsByShiftId fetches all duty assignments by shiftId
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

// CreateDutyAssignments creates duty assignments for a Shift based on Role
func (h *DutyAssignmentHandler) CreateDutyAssignments(w http.ResponseWriter, r *http.Request) {
	// Parse request body
	var request struct {
		ShiftId string `json:"ShiftId"`
		RoleId  string `json:"RoleId"`
	}

	if err := json.NewDecoder(r.Body).Decode(&request); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Validate and parse ShiftId and RoleId
	shiftId, err := uuid.Parse(request.ShiftId)
	if err != nil {
		http.Error(w, "Invalid 'ShiftId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	roleId, err := uuid.Parse(request.RoleId)
	if err != nil {
		http.Error(w, "Invalid 'RoleId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Call service to create duty assignments
	if err := h.service.CreateDutyAssignments(context.Background(), shiftId, roleId); err != nil {
		http.Error(w, "Failed to create duty assignments: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Send success response
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)

	response := map[string]string{"message": "Duty assignments created successfully"}
	json.NewEncoder(w).Encode(response)
}

// UpdateDutyAssignment updates a duty assignment
func (h *DutyAssignmentHandler) UpdateDutyAssignment(w http.ResponseWriter, r *http.Request) {
	// Extract ShiftId and DutyId from path parameters
	vars := mux.Vars(r)
	shiftIdStr := vars["ShiftId"]
	dutyIdStr := vars["DutyId"]

	// Validate presence of path parameters
	if shiftIdStr == "" || dutyIdStr == "" {
		http.Error(w, "Missing 'ShiftId' or 'DutyId' path parameter", http.StatusBadRequest)
		return
	}

	w.Header().Set("Content-Type", "application/json")

	// Parse the ShiftId and DutyId from string to uuid.UUID
	shiftIdUUID, err := uuid.Parse(shiftIdStr)
	if err != nil {
		http.Error(w, "Invalid 'ShiftId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	dutyIdUUID, err := uuid.Parse(dutyIdStr)
	if err != nil {
		http.Error(w, "Invalid 'DutyId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Parse the request body to get the updated DutyAssignment
	var updatedDutyAssignment models.DutyAssignment
	if err := json.NewDecoder(r.Body).Decode(&updatedDutyAssignment); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Validate DutyAssignmentStatus
	if !models.ValidateDutyAssignmentStatus(updatedDutyAssignment.DutyAssignmentStatus) {
		http.Error(w, "Invalid DutyAssignmentStatus. Valid values are 'Completed' or 'Incomplete'.", http.StatusBadRequest)
		return
	}

	// Ensure ShiftId and DutyId match the updatedDutyAssignment object
	// Compare UUID values instead of strings
	if updatedDutyAssignment.PartitionKey != shiftIdUUID || updatedDutyAssignment.RowKey != dutyIdUUID {
		http.Error(w, "Mismatched ShiftId or DutyId in request body. The ShiftId and DutyId in the request should match the ShiftId and DutyId in the body.", http.StatusBadRequest)
		return
	}

	// Call the service method to update the duty assignment
	if err := h.service.UpdateDutyAssignment(context.Background(), updatedDutyAssignment); err != nil {
		http.Error(w, "Failed to update duty assignment: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusOK) // 200 OK

	// Send a success message as JSON
	response := map[string]string{"message": "Duty assignment updated successfully"}
	json.NewEncoder(w).Encode(response)
}

// DeleteDutyAssignment deletes a duty assignment
func (h *DutyAssignmentHandler) DeleteDutyAssignment(w http.ResponseWriter, r *http.Request) {
	// Extract ShiftId and DutyId from path parameters
	vars := mux.Vars(r)
	shiftIdStr := vars["ShiftId"]
	dutyIdStr := vars["DutyId"]

	// Validate presence of path parameters
	if shiftIdStr == "" || dutyIdStr == "" {
		http.Error(w, "Missing 'ShiftId' or 'DutyId' path parameter", http.StatusBadRequest)
		return
	}

	// Parse the shiftId from string to uuid.UUID
	shiftIdUUID, err := uuid.Parse(shiftIdStr)
	if err != nil {
		http.Error(w, "Invalid 'shiftId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	dutyIdUUID, err := uuid.Parse(dutyIdStr)
	if err != nil {
		http.Error(w, "Invalid 'dutyId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	// Call the service method to delete the duty assignment
	if err := h.service.DeleteDutyAssignment(context.Background(), shiftIdUUID, dutyIdUUID); err != nil {
		http.Error(w, "Failed to delete duty assignment: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Send success response
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK) // 200 OK

	// Send a success message as JSON
	response := map[string]string{"message": "Duty assignment deleted successfully"}
	json.NewEncoder(w).Encode(response)
}
