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

type DutyHandler struct {
	service services.InterfaceDutyService
}

// NewDutyHandler creates a new DutyHandler
func NewDutyHandler(service services.InterfaceDutyService) *DutyHandler {
	return &DutyHandler{service: service}
}

func (h *DutyHandler) GetAllDuties(w http.ResponseWriter, r *http.Request) {
	query := r.URL.Query()
	name := query.Get("name") // Get the name parameter from the query string

	duties, err := h.service.GetAllDuties(context.Background(), name)
	if err != nil {
		http.Error(w, "Failed to retrieve duties: "+err.Error(), http.StatusInternalServerError)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(duties)
}

func (h *DutyHandler) GetDutyById(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	partitionKey := vars["PartitionKey"]
	rowKey := vars["RowKey"]

	if partitionKey == "" || rowKey == "" {
		http.Error(w, "Missing PartitionKey or RrowKey", http.StatusBadRequest)
		return
	}

	duty, err := h.service.GetDutyById(context.Background(), partitionKey, rowKey)
	if err != nil {
		http.Error(w, "Failed to retrieve duty: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(duty)
}

func (h *DutyHandler) GetDutiesByRole(w http.ResponseWriter, r *http.Request) {
	query := r.URL.Query()
	roleId := query.Get("RoleId") // Extract the RoleId from query parameters

	if roleId == "" {
		http.Error(w, "Missing 'RoleId' query parameter", http.StatusBadRequest)
		return
	}

	roleIdUUID, err := uuid.Parse(roleId) // Validate the UUID format
	if err != nil {
		http.Error(w, "Invalid 'RoleId' format: "+err.Error(), http.StatusBadRequest)
		return
	}

	duties, err := h.service.GetDutiesByRole(context.Background(), roleIdUUID)
	if err != nil {
		http.Error(w, "Failed to retrieve duties by RoleId: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(duties)
}

func (h *DutyHandler) CreateDuty(w http.ResponseWriter, r *http.Request) {
	var duty models.Duty
	if err := json.NewDecoder(r.Body).Decode(&duty); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	if duty.PartitionKey == "" {
		duty.PartitionKey = "Duty"
	}

	duty.RowKey = uuid.New() // TODO: make these unique

	if err := h.service.CreateDuty(context.Background(), duty); err != nil {
		http.Error(w, "Failed to create duty: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(map[string]string{"message": "duty created successfully"})
}

func (h *DutyHandler) UpdateDuty(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	partitionKey := vars["PartitionKey"]
	rowKey := vars["RowKey"]

	var duty models.Duty

	if err := json.NewDecoder(r.Body).Decode(&duty); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	// Set Content-Type to application/json for the response
	w.Header().Set("Content-Type", "application/json")

	if err := h.service.UpdateDuty(context.Background(), partitionKey, rowKey, duty); err != nil {
		http.Error(w, "Failed to update duty: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "Duty updated successfully"})
}

func (h *DutyHandler) DeleteDuty(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	partitionKey := vars["PartitionKey"]
	rowKey := vars["RowKey"]

	// Set the Content-Type header for JSON responses
	w.Header().Set("Content-Type", "application/json")

	err := h.service.DeleteDuty(context.Background(), partitionKey, rowKey)
	if err != nil {
		if err.Error() == "ResourceNotFound" {
			http.Error(w, `{"error": "Duty not found"}`, http.StatusNotFound)
			return
		}
		http.Error(w, `{"error": "Failed to delete duty: `+err.Error()+`"}`, http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "Duty deleted successfully"})
}
