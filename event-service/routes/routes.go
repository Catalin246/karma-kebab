package routes

import (
	"net/http"

	"github.com/Catalin246/karma-kebab/handlers"
	"github.com/Catalin246/karma-kebab/middlewares"
	"github.com/Catalin246/karma-kebab/repositories"
	"github.com/Catalin246/karma-kebab/services"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/gorilla/mux"
)

// RegisterRoutes registers all the routes for the event service
func RegisterRoutes(serviceClient *aztables.ServiceClient, rabbitMQService services.RabbitMQServiceInterface) *mux.Router {
	// Create the repository and service instances
	eventRepository := repositories.NewEventRepository(serviceClient)
	eventService := services.NewEventService(eventRepository)

	// Create the event handler and inject the service and RabbitMQService
	eventHandler := handlers.NewEventHandler(eventService, rabbitMQService)

	r := mux.NewRouter()

	// Apply the middleware to all routes
	r.Use(middlewares.GatewayHeaderMiddleware)

	// Event routes
	r.HandleFunc("/events", eventHandler.GetEvents).Methods(http.MethodGet)
	r.HandleFunc("/events/{partitionKey}/{rowKey}", eventHandler.GetEventByID).Methods(http.MethodGet)
	r.HandleFunc("/events", eventHandler.CreateEvent).Methods(http.MethodPost)
	r.HandleFunc("/events/{partitionKey}/{rowKey}", eventHandler.UpdateEvent).Methods(http.MethodPut)
	r.HandleFunc("/events/{partitionKey}/{rowKey}", eventHandler.DeleteEvent).Methods(http.MethodDelete)
	r.HandleFunc("/events/shift/{shiftID}", eventHandler.GetEventByShiftID).Methods(http.MethodGet)

	return r
}
