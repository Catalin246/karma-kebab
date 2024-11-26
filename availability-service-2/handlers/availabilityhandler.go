package handlers

import (
	"availability-service-2/models"
	"availability-service-2/service"
	"encoding/json"
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

    availabilities, err := h.service.GetAll(r.Context(),  startDate, endDate)
    if err != nil {
        http.Error(w, err.Error(), http.StatusInternalServerError)
        return
    }

    w.Header().Set("Content-Type", "application/json")
    json.NewEncoder(w).Encode(availabilities)
}

// GetByID retrieves a specific availability record by EmployeeID.
func (h *AvailabilityHandler) GetByEmployeeID(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	employeeID := vars["employeeId"]

	// Ensure employee ID is provided
	if employeeID == "" {
		http.Error(w, "EmployeeID is required", http.StatusBadRequest)
		return
	}

	availability, err := h.service.GetByEmployeeID(r.Context(), employeeID)
	if err != nil {
		if err == models.ErrNotFound {
			http.Error(w, "Availability not found", http.StatusNotFound)
		} else {
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(availability)
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
