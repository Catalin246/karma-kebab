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

func NewDutyAssignmentHandler(service services.InterfaceDutyAssignmentService) *DutyAssignmentHandler {
	return &DutyAssignmentHandler{service: service}
}

// fetch all duty assignments by shiftId
func (h *DutyAssignmentHandler) GetAllDutyAssignmentsByShiftId(w http.ResponseWriter, r *http.Request) {
	query := r.URL.Query()
	shiftIdStr := query.Get("shiftId") // Get the shiftId parameter from the query string

	if shiftIdStr == "" { // check that the shiftId parameter is provided and is valid
		http.Error(w, "Missing 'shiftId' query parameter", http.StatusBadRequest)
		return
	}

	shiftIdUUID, err := uuid.Parse(shiftIdStr) // parse the shiftId from string to uuid.UUID
	if err != nil {
		http.Error(w, "Invalid 'shiftId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	// fetch duty assignments by shiftId via service
	dutyAssignments, err := h.service.GetAllDutyAssignmentsByShiftId(context.Background(), shiftIdUUID)
	if err != nil {
		http.Error(w, "Failed to retrieve duty assignments: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	err = json.NewEncoder(w).Encode(dutyAssignments)
	if err != nil {
		http.Error(w, "Failed to encode response: "+err.Error(), http.StatusInternalServerError)
	}
}

// creates duty assignments for a Shift based on Role
func (h *DutyAssignmentHandler) CreateDutyAssignments(w http.ResponseWriter, r *http.Request) {
	// parse request body
	var request struct {
		ShiftId string `json:"ShiftId"`
		RoleId  string `json:"RoleId"`
	}

	if err := json.NewDecoder(r.Body).Decode(&request); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}

	shiftId, err := uuid.Parse(request.ShiftId) // Validate and parse ShiftId and RoleId
	if err != nil {
		http.Error(w, "Invalid 'ShiftId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	roleId, err := uuid.Parse(request.RoleId)
	if err != nil {
		http.Error(w, "Invalid 'RoleId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	if err := h.service.CreateDutyAssignments(context.Background(), shiftId, roleId); err != nil {
		http.Error(w, "Failed to create duty assignments: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)

	response := map[string]string{"message": "Duty assignments created successfully"}
	json.NewEncoder(w).Encode(response)
}

// updates a duty assignment
func (h *DutyAssignmentHandler) UpdateDutyAssignment(w http.ResponseWriter, r *http.Request) {
	// get ShiftId and DutyId from path parameters
	vars := mux.Vars(r)
	shiftIdStr := vars["ShiftId"]
	dutyIdStr := vars["DutyId"]

	// check for path parameters
	if shiftIdStr == "" || dutyIdStr == "" {
		http.Error(w, "Missing 'ShiftId' or 'DutyId' path parameter", http.StatusBadRequest)
		return
	}

	w.Header().Set("Content-Type", "application/json")

	shiftIdUUID, err := uuid.Parse(shiftIdStr) // parse string to uuid.UUID
	if err != nil {
		http.Error(w, "Invalid 'ShiftId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	dutyIdUUID, err := uuid.Parse(dutyIdStr)
	if err != nil {
		http.Error(w, "Invalid 'DutyId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	// parse the request body to get the updated DutyAssignment
	var updatedDutyAssignment models.DutyAssignment
	if err := json.NewDecoder(r.Body).Decode(&updatedDutyAssignment); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}
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

	if err := h.service.UpdateDutyAssignment(context.Background(), updatedDutyAssignment); err != nil {
		http.Error(w, "Failed to update duty assignment: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusOK) // 200 OK
	response := map[string]string{"message": "Duty assignment updated successfully"}
	json.NewEncoder(w).Encode(response)
}

// deletes a duty assignment
func (h *DutyAssignmentHandler) DeleteDutyAssignment(w http.ResponseWriter, r *http.Request) {
	// get ShiftId and DutyId from path parameters
	vars := mux.Vars(r)
	shiftIdStr := vars["ShiftId"]
	dutyIdStr := vars["DutyId"]

	// cehck path parameters
	if shiftIdStr == "" || dutyIdStr == "" {
		http.Error(w, "Missing 'ShiftId' or 'DutyId' path parameter", http.StatusBadRequest)
		return
	}

	shiftIdUUID, err := uuid.Parse(shiftIdStr) // Parse the shiftId from string to uuid.UUID
	if err != nil {
		http.Error(w, "Invalid 'shiftId' format: "+err.Error(), http.StatusBadRequest)
		return
	}
	dutyIdUUID, err := uuid.Parse(dutyIdStr)
	if err != nil {
		http.Error(w, "Invalid 'dutyId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	if err := h.service.DeleteDutyAssignment(context.Background(), shiftIdUUID, dutyIdUUID); err != nil {
		http.Error(w, "Failed to delete duty assignment: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK) // 200 OK

	response := map[string]string{"message": "Duty assignment deleted successfully"}
	json.NewEncoder(w).Encode(response)
}
