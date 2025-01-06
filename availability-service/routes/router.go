// routes/routes.go
package routes

import (
	"availability-service/handlers"
	"availability-service/middlewares"
	"availability-service/repository"
	"availability-service/service"
	"availability-service/models"
	"context"
	"encoding/json"
	"log"
	"net/http"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/gorilla/mux"
)

// Struct to represent the shift availability request
type ShiftAvailabilityRequest struct {
	Date string `json:"date"`
}

// Response struct to send back to shift service
type AvailabilityResponse struct {
	AvailableEmployeeIDs []string `json:"availableEmployeeIDs"`
}

func RegisterRoutes(serviceClient *aztables.ServiceClient, rabbitMQService *service.RabbitMQService) *mux.Router {
	// Set up queues
	if err := rabbitMQService.SetupQueues(); err != nil {
		log.Fatalf("Failed to setup RabbitMQ queues: %v", err)
	}

	// Create the repository and service instances
	availabilityRepository := repository.NewTableStorageAvailabilityRepository(serviceClient)
	availabilityService := service.NewAvailabilityService(availabilityRepository)

	// Create the availability handler and inject the service
	availabilityHandler := handlers.NewAvailabilityHandler(availabilityService)

	// Create a new Gorilla Mux router
	r := mux.NewRouter()

	// Apply middleware to all routes
	r.Use(middlewares.GatewayHeaderMiddleware)

	// Existing availability routes...
	r.HandleFunc("/availability", availabilityHandler.GetAll).Methods(http.MethodGet)
	r.HandleFunc("/availability/{partitionKey}", availabilityHandler.GetByEmployeeID).Methods(http.MethodGet)
	r.HandleFunc("/availability", availabilityHandler.Create).Methods(http.MethodPost)
	r.HandleFunc("/availability/{partitionKey}/{rowKey}", availabilityHandler.Update).Methods(http.MethodPut)
	r.HandleFunc("/availability/{partitionKey}/{rowKey}", availabilityHandler.Delete).Methods(http.MethodDelete)

	err := rabbitMQService.ConsumeMessages(service.ShiftAvailabilityRequestQueue, func(body []byte) error {
		// Parse the incoming request
		var request ShiftAvailabilityRequest
		if err := json.Unmarshal(body, &request); err != nil {
			return err
		}

		// Parse the date
		shiftStart, err := time.Parse(time.RFC3339, request.Date)
		if err != nil {
			return err
		}

		// Assuming shift duration is a fixed time, e.g., 8 hours
		shiftEnd := shiftStart.Add(8 * time.Hour)

		// Create a context
		ctx := context.Background()

		// Get availability records for the specific date range
		availabilityRecords, err := availabilityService.GetAll(ctx, &shiftStart, &shiftEnd)
		if err != nil {
			return err
		}

		// Extract available employee IDs
		availableEmployeeIDs := extractAvailableEmployeeIDs(availabilityRecords, shiftStart, shiftEnd)

		// Prepare response
		response := AvailabilityResponse{
			AvailableEmployeeIDs: availableEmployeeIDs,
		}

		// Serialize the response
		responseBody, err := json.Marshal(response)
		if err != nil {
			return err
		}

		// Publish the response back to the shift service
		return rabbitMQService.PublishMessage(service.ShiftAvailabilityResponseQueue, responseBody)
	})

	if err != nil {
		log.Fatalf("Failed to set up message consumption: %v", err)
	}

	http.Handle("/", r)
	return r
}

// Helper function to extract available employee IDs
func extractAvailableEmployeeIDs(availabilityRecords []models.Availability, shiftStart, shiftEnd time.Time) []string {
    // Create a map to track which employees are unavailable
    unavailableEmployees := make(map[string]bool)

    for _, record := range availabilityRecords {
        // Check if the record represents an unavailability (overlaps with shift time)
        if isUnavailabilityOverlapping(record, shiftStart, shiftEnd) {
            unavailableEmployees[record.EmployeeID] = true
        }
    }

    // Collect unique available employee IDs
    availableEmployeeIDs := make([]string, 0)
    employeeIDSet := make(map[string]bool)

    for _, record := range availabilityRecords {
        // Only add unique employees who are not unavailable
        if _, unavailable := unavailableEmployees[record.EmployeeID]; !unavailable {
            if _, exists := employeeIDSet[record.EmployeeID]; !exists {
                availableEmployeeIDs = append(availableEmployeeIDs, record.EmployeeID)
                employeeIDSet[record.EmployeeID] = true
            }
        }
    }

    return availableEmployeeIDs
}


func isUnavailabilityOverlapping(record models.Availability, shiftStart, shiftEnd time.Time) bool {
    // Check for time overlap
    // Overlap occurs if:
    // 1. Unavailability start is before shift end AND
    // 2. Unavailability end is after shift start
    return record.StartDate.Before(shiftEnd) && record.EndDate.After(shiftStart)
}