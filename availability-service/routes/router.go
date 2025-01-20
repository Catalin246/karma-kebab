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

type ShiftAvailabilityRequest struct {
    Date       string `json:"date"`
    EmployeeID string `json:"employeeId,omitempty"` // Added optional employee ID filter
}

type AvailabilityResponse struct {
    AvailableEmployeeIDs []string `json:"availableEmployeeIDs"`
}

func RegisterRoutes(serviceClient *aztables.ServiceClient, rabbitMQService *service.RabbitMQService) *mux.Router {
    // Set up queues
    if err := rabbitMQService.SetupQueues(); err != nil {
        log.Fatalf("Failed to setup RabbitMQ queues: %v", err)
    }

    availabilityRepository := repository.NewTableStorageAvailabilityRepository(serviceClient)
    availabilityService := service.NewAvailabilityService(availabilityRepository)

    availabilityHandler := handlers.NewAvailabilityHandler(availabilityService)

    r := mux.NewRouter()

    r.Use(middlewares.GatewayHeaderMiddleware)

    r.HandleFunc("/availability", availabilityHandler.GetAll).Methods(http.MethodGet)
    r.HandleFunc("/availability", availabilityHandler.Create).Methods(http.MethodPost)
    r.HandleFunc("/availability/{partitionKey}/{rowKey}", availabilityHandler.Update).Methods(http.MethodPut)
    r.HandleFunc("/availability/{partitionKey}/{rowKey}", availabilityHandler.Delete).Methods(http.MethodDelete)

    err := rabbitMQService.ConsumeMessages(service.ShiftAvailabilityRequestQueue, func(body []byte) error {
        var request ShiftAvailabilityRequest
        if err := json.Unmarshal(body, &request); err != nil {
            return err
        }

        shiftStart, err := time.Parse(time.RFC3339, request.Date)
        if err != nil {
            return err
        }

        shiftEnd := shiftStart.Add(8 * time.Hour)

        ctx := context.Background()

        availabilityRecords, err := availabilityService.GetAll(ctx, request.EmployeeID, &shiftStart, &shiftEnd)
        if err != nil {
            return err
        }

        availableEmployeeIDs := extractAvailableEmployeeIDs(availabilityRecords, shiftStart, shiftEnd)

        if request.EmployeeID != "" {
            filteredIDs := make([]string, 0)
            for _, id := range availableEmployeeIDs {
                if id == request.EmployeeID {
                    filteredIDs = append(filteredIDs, id)
                    break
                }
            }
            availableEmployeeIDs = filteredIDs
        }

        response := AvailabilityResponse{
            AvailableEmployeeIDs: availableEmployeeIDs,
        }

        responseBody, err := json.Marshal(response)
        if err != nil {
            return err
        }

        return rabbitMQService.PublishMessage(service.ShiftAvailabilityResponseQueue, responseBody)
    })

    if err != nil {
        log.Fatalf("Failed to set up message consumption: %v", err)
    }

    http.Handle("/", r)
    return r
}

func extractAvailableEmployeeIDs(availabilityRecords []models.Availability, shiftStart, shiftEnd time.Time) []string {
    unavailableEmployees := make(map[string]bool)

    for _, record := range availabilityRecords {
        // Checks if the record represents an unavailability (overlaps with shift time)
        if isUnavailabilityOverlapping(record, shiftStart, shiftEnd) {
            unavailableEmployees[record.EmployeeID] = true
        }
    }

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