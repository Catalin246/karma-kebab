package handlers

import (
	"availability-service/models"
	"context"
	"encoding/json"
	"net/http"
	"strings"
	"time"

	"github.com/google/uuid"
	"github.com/gorilla/mux"
)

// AvailabilityServiceInterface defines the methods that the service must implement
type IAvailability interface {
	GetAll(ctx context.Context, employeeID string, startDate, endDate *time.Time) ([]models.Availability, error)
	Create(ctx context.Context, availability models.Availability) (*models.Availability, error)
	Update(ctx context.Context, employeeID, id string, availability models.Availability) error
	Delete(ctx context.Context, employeeID, id string) error
}

type AvailabilityHandler struct {
	service IAvailability
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

// NewAvailabilityHandler now accepts the interface instead of a pointer to the concrete service
func NewAvailabilityHandler(service IAvailability) *AvailabilityHandler {
	return &AvailabilityHandler{
		service: service,
	}
}

func (h *AvailabilityHandler) GetAll(w http.ResponseWriter, r *http.Request) {
    employeeID := r.URL.Query().Get("employeeId")
    startDateStr := r.URL.Query().Get("startDate")
    endDateStr := r.URL.Query().Get("endDate")

    var startDate, endDate *time.Time

    // Validate employeeID as a valid UUID
    if employeeID != "" {
        if _, err := uuid.Parse(employeeID); err != nil {
            http.Error(w, "Invalid employeeId format. Must be a valid UUID.", http.StatusBadRequest)
            return
        }
    }

    // Define multiple date formats to try
    dateFormats := []string{
        time.RFC3339,
        "2006-01-02T15:04:05Z07:00",
        "2006-01-02T15:04:05Z",
        "2006-01-02",
    }

    if startDateStr != "" {
        // Remove quotes if they exist
        startDateStr = strings.Trim(startDateStr, "\"")

        var parsedStartDate time.Time
        var err error

        // Try parsing with different formats
        for _, format := range dateFormats {
            parsedStartDate, err = time.Parse(format, startDateStr)
            if err == nil {
                break
            }
        }

        if err != nil {
            http.Error(w, "Invalid startDate format. Use RFC3339 format.", http.StatusBadRequest)
            return
        }

        startDate = &parsedStartDate
    }

    if endDateStr != "" {
        // Remove quotes if they exist
        endDateStr = strings.Trim(endDateStr, "\"")

        var parsedEndDate time.Time
        var err error

        // Try parsing with different formats
        for _, format := range dateFormats {
            parsedEndDate, err = time.Parse(format, endDateStr)
            if err == nil {
                break
            }
        }

        if err != nil {
            http.Error(w, "Invalid endDate format. Use RFC3339 format.", http.StatusBadRequest)
            return
        }

        endDate = &parsedEndDate
    }

    availabilities, err := h.service.GetAll(r.Context(), employeeID, startDate, endDate)
    if err != nil {
        http.Error(w, err.Error(), http.StatusInternalServerError)
        return
    }

    w.Header().Set("Content-Type", "application/json")
    if err := json.NewEncoder(w).Encode(availabilities); err != nil {
        http.Error(w, err.Error(), http.StatusInternalServerError)
    }
}

func (h *AvailabilityHandler) Create(w http.ResponseWriter, r *http.Request) {
	var availability models.Availability
	if err := json.NewDecoder(r.Body).Decode(&availability); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	// Capture the returned availability
	createdAvailability, err := h.service.Create(r.Context(), availability)
	if err != nil {
		if strings.Contains(err.Error(), "availability conflicts") {
			http.Error(w, err.Error(), http.StatusConflict)
			return
		}
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	// Respond with created availability
	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(createdAvailability)
}

func (h *AvailabilityHandler) Update(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	partitionKey := vars["partitionKey"] // EmployeeID
	rowKey := vars["rowKey"]             // Availability ID

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
		ID:         rowKey,
		EmployeeID: partitionKey,
		StartDate:  startDate,
		EndDate:    endDate,
	}

	err = h.service.Update(r.Context(), partitionKey, rowKey, availability)
	if err != nil {
		switch err {
		case models.ErrNotFound:
			http.Error(w, "Availability not found", http.StatusNotFound)
		case models.ErrInvalidID:
			http.Error(w, "Invalid employee ID or availability ID", http.StatusBadRequest)
		default:
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}
		return
	}

	w.WriteHeader(http.StatusOK)
}

// delete
func (h *AvailabilityHandler) Delete(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	partitionKey := vars["partitionKey"] // EmployeeID
	rowKey := vars["rowKey"]             // Availability ID

	if partitionKey == "" || rowKey == "" {
		http.Error(w, "EmployeeID and Availability ID are required", http.StatusBadRequest)
		return
	}

	err := h.service.Delete(r.Context(), partitionKey, rowKey)
	if err != nil {
		switch err {
		case models.ErrNotFound:
			http.Error(w, "Availability not found", http.StatusNotFound)
		case models.ErrInvalidID:
			http.Error(w, "Invalid employee ID or availability ID", http.StatusBadRequest)
		default:
			http.Error(w, err.Error(), http.StatusInternalServerError)
		}
		return
	}

	w.WriteHeader(http.StatusNoContent)
}
