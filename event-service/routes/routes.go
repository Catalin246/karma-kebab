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
func RegisterRoutes(serviceClient *aztables.ServiceClient, rabbitMQService services.RabbitMQServiceInterface, publicKeyPEM string) *mux.Router {
	// Create the repository and service instances
	eventRepository := repositories.NewEventRepository(serviceClient)
	eventService := services.NewEventService(eventRepository)

	// Create the event handler and inject the service and RabbitMQService
	eventHandler := handlers.NewEventHandler(eventService, rabbitMQService)
	metricsHandler := handlers.NewMetricsHandler()

	r := mux.NewRouter()

	// Apply the middleware to all routes
	r.Use(middlewares.GatewayHeaderMiddleware)

	//metrics routes:
	r.HandleFunc("/events/metrics", metricsHandler.HandleMetrics).Methods(http.MethodGet)

	// Event routes
	r.HandleFunc("/events", eventHandler.GetEvents).Methods(http.MethodGet)
	r.HandleFunc("/events/{partitionKey}/{rowKey}", eventHandler.GetEventByID).Methods(http.MethodGet)
	//r.HandleFunc("/events", eventHandler.CreateEvent).Methods(http.MethodPost)
	r.Handle("/events", middlewares.JWTMiddleware(publicKeyPEM, http.HandlerFunc(eventHandler.CreateEvent))).Methods(http.MethodPost) //require Admin role to create event
	//r.HandleFunc("/events/{partitionKey}/{rowKey}", eventHandler.UpdateEvent).Methods(http.MethodPut)
	r.Handle("/events/{partitionKey}/{rowKey}", middlewares.JWTMiddleware(publicKeyPEM, http.HandlerFunc(eventHandler.UpdateEvent))).Methods(http.MethodPut) //require Admin role to update event
	//r.HandleFunc("/events/{partitionKey}/{rowKey}", eventHandler.DeleteEvent).Methods(http.MethodDelete)
	r.Handle("/events/{partitionKey}/{rowKey}", middlewares.JWTMiddleware(publicKeyPEM, http.HandlerFunc(eventHandler.DeleteEvent))).Methods(http.MethodDelete) //require Admin role to delete event
	r.HandleFunc("/events/{shiftID}", eventHandler.GetEventByShiftID).Methods(http.MethodGet)

	return r
}
