package handlers

import (
	"availability-service-2/models"
	"availability-service-2/service"
	"encoding/json"
	"log"
	"net/http"
	"time"

	"github.com/gorilla/mux"
)

type AvailabilityHandler struct {
	service *service.AvailabilityService
}

type CreateAvailabilityRequest struct {
	EmployeeID string `json:"employeeId"`
	StartDate  string `json:"startDate"`
	EndDate    string `json:"endDate"`
}

type UpdateAvailabilityRequest struct {
	EmployeeID string `json:"employeeId"`
	StartDate  string `json:"startDate"`
	EndDate    string `json:"endDate"`
}

func NewAvailabilityHandler(service *service.AvailabilityService) *AvailabilityHandler {
	return &AvailabilityHandler{
		service: service,
	}
}

// GetAll retrieves all availability records for all employees.
func (h *AvailabilityHandler) GetAll(w http.ResponseWriter, r *http.Request) {
	startDateStr := r.URL.Query().Get("startDate")
	endDateStr := r.URL.Query().Get("endDate")
	log.Println("Received GET all request:")

	var startDate, endDate *time.Time
	if startDateStr != "" {
		parsedStartDate, err := time.Parse(time.RFC3339, startDateStr)
		if err != nil {
			http.Error(w, "Invalid startDate format", http.StatusBadRequest)
			return
		}
		startDate = &parsedStartDate
	}
	if endDateStr != "" {
		parsedEndDate, err := time.Parse(time.RFC3339, endDateStr)
		if err != nil {
			http.Error(w, "Invalid endDate format", http.StatusBadRequest)
			return
		}
		endDate = &parsedEndDate
	}

	availabilities, err := h.service.GetAll(r.Context(), startDate, endDate)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(availabilities)
}

// gets all availabilities of one employee
func (h *AvailabilityHandler) GetByEmployeeID(w http.ResponseWriter, r *http.Request) {
	// Log full request details for debugging
	log.Printf("Received GETby emp id request: %+v", r)

	// Log all URL variables
	vars := mux.Vars(r)
	log.Printf("URL Variables: %+v", vars)

	// Use partitionKey instead of employeeId
	partitionKey := vars["partitionKey"]
	log.Printf("Extracted PartitionKey (EmployeeID): '%s'", partitionKey)

	// Ensure partition key is provided
	if partitionKey == "" {
		log.Println("Error: PartitionKey is empty")
		http.Error(w, "PartitionKey is required", http.StatusBadRequest)
		return
	}

	availabilities, err := h.service.GetByEmployeeID(r.Context(), partitionKey)
	if err != nil {
		log.Printf("Service Error: %v", err)
		if err == models.ErrNotFound {
			http.Error(w, "Availability not found", http.StatusNotFound)
		} else {
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}
		return
	}

	// Log the retrieved availabilities
	log.Printf("Retrieved Availabilities: %+v", availabilities)

	// Check if no availabilities found
	if len(availabilities) == 0 {
		http.Error(w, "No availabilities found for this employee", http.StatusNotFound)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	if err := json.NewEncoder(w).Encode(availabilities); err != nil {
		log.Printf("JSON Encoding Error: %v", err)
		http.Error(w, "Error encoding response", http.StatusInternalServerError)
	}
}

// Create creates a new availability record.
func (h *AvailabilityHandler) Create(w http.ResponseWriter, r *http.Request) {
	var req CreateAvailabilityRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	startDate, err := time.Parse(time.RFC3339, req.StartDate)
	if err != nil {
		http.Error(w, "Invalid start date format", http.StatusBadRequest)
		return
	}

	endDate, err := time.Parse(time.RFC3339, req.EndDate)
	if err != nil {
		http.Error(w, "Invalid end date format", http.StatusBadRequest)
		return
	}

	availability := models.Availability{
		EmployeeID: req.EmployeeID,
		StartDate:  startDate,
		EndDate:    endDate,
	}

	created, err := h.service.Create(r.Context(), availability)
	if err != nil {
		switch err {
		case models.ErrInvalidAvailability:
			http.Error(w, err.Error(), http.StatusBadRequest)
		case models.ErrConflict:
			http.Error(w, err.Error(), http.StatusConflict)
		default:
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(created)
}

// Update updates an existing availability record.
func (h *AvailabilityHandler) Update(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	id := vars["id"]

	var req UpdateAvailabilityRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	startDate, err := time.Parse(time.RFC3339, req.StartDate)
	if err != nil {
		http.Error(w, "Invalid start date format", http.StatusBadRequest)
		return
	}

	endDate, err := time.Parse(time.RFC3339, req.EndDate)
	if err != nil {
		http.Error(w, "Invalid end date format", http.StatusBadRequest)
		return
	}

	availability := models.Availability{
		ID:         id,
		EmployeeID: req.EmployeeID,
		StartDate:  startDate,
		EndDate:    endDate,
	}

	err = h.service.Update(r.Context(), req.EmployeeID, id, availability)
	if err != nil {
		switch err {
		case models.ErrNotFound:
			http.Error(w, "Availability not found", http.StatusNotFound)
		default:
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(availability)
}

// Delete deletes an availability record.
func (h *AvailabilityHandler) Delete(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	id := vars["id"]
	employeeID := r.URL.Query().Get("employeeId")

	if employeeID == "" {
		http.Error(w, "EmployeeID is required", http.StatusBadRequest)
		return
	}

	err := h.service.Delete(r.Context(), employeeID, id)
	if err != nil {
		switch err {
		case models.ErrNotFound:
			http.Error(w, "Availability not found", http.StatusNotFound)
		default:
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}
		return
	}

	w.WriteHeader(http.StatusNoContent)
}
