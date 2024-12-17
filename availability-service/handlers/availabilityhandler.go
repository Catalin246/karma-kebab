package handlers

import (
	"availability-service/models"
	"context"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"strconv"
	"strings"
	"time"

	"github.com/gorilla/mux"
)

// AvailabilityServiceInterface defines the methods that the service must implement
type IAvailability interface {
	GetAll(ctx context.Context, startDate, endDate *time.Time, roleIDs []int) ([]models.Availability, error)
	GetByEmployeeID(ctx context.Context, employeeID string) ([]models.Availability, error)
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
	RoleIDs    []int  `json:"roleIds"`
}

type UpdateAvailabilityRequest struct {
	EmployeeID string `json:"employeeId"`
	StartDate  string `json:"startDate"`
	EndDate    string `json:"endDate"`
	RoleIDs    []int  `json:"roleIds"`
}

// NewAvailabilityHandler now accepts the interface instead of a pointer to the concrete service
func NewAvailabilityHandler(service IAvailability) *AvailabilityHandler {
	return &AvailabilityHandler{
		service: service,
	}
}

func (h *AvailabilityHandler) GetAll(w http.ResponseWriter, r *http.Request) {
	startDateStr := r.URL.Query().Get("startDate")
	endDateStr := r.URL.Query().Get("endDate")
	roleIDsStr := r.URL.Query().Get("RoleIDs")
	var roleIDs []int
	if roleIDsStr != "" {
		parsedRoleIDs, err := parseRoleIDs(roleIDsStr)
		if err != nil {
			http.Error(w, err.Error(), http.StatusBadRequest)
			return
		}
		roleIDs = parsedRoleIDs
	}

	var startDate, endDate *time.Time

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

	availabilities, err := h.service.GetAll(r.Context(), startDate, endDate, roleIDs)
	if err != nil {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(availabilities)
}

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

func (h *AvailabilityHandler) Create(w http.ResponseWriter, r *http.Request) {
	var req CreateAvailabilityRequest
	if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	// Parse start and end dates
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

	// Create Availability model
	availability := models.Availability{
		EmployeeID: req.EmployeeID,
		StartDate:  startDate,
		EndDate:    endDate,
		RoleIDs:    req.RoleIDs,
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
		RoleIDs:    req.RoleIDs,
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

func parseRoleIDs(roleIDsStr string) ([]int, error) {
    // If the string is empty, return an empty slice
    if roleIDsStr == "" {
        return nil, nil
    }

    // Split the string by comma
    roleIDStrings := strings.Split(roleIDsStr, ",")
    
    // Create a slice to store parsed role IDs
    roleIDs := make([]int, 0, len(roleIDStrings))
    
    // Parse each string to an integer
    for _, idStr := range roleIDStrings {
        // Trim any whitespace
        idStr = strings.TrimSpace(idStr)
        
        // Convert to integer
        roleID, err := strconv.Atoi(idStr)
        if err != nil {
            // Return an error if any ID is not a valid integer
            return nil, fmt.Errorf("invalid role ID: %s", idStr)
        }
        
        roleIDs = append(roleIDs, roleID)
    }
    
    return roleIDs, nil
}