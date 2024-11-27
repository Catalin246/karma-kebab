package handlers

import (
	"context"
	"duty-service/models"
	"duty-service/services"
	"encoding/json"
	"net/http"

	"github.com/google/uuid"
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

func (h *DutyHandler) CreateDuty(w http.ResponseWriter, r *http.Request) {
	var duty models.Duty
	if err := json.NewDecoder(r.Body).Decode(&duty); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	// Set default PartitionKey if not provided
	if duty.PartitionKey == "" {
		duty.PartitionKey = "Duty"
	}

	duty.RowKey = uuid.New() //TODO: make these unique

	if err := h.service.CreateDuty(context.Background(), duty); err != nil {
		http.Error(w, "Failed to create duty: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(map[string]string{"message": "duty created successfully"})
}
