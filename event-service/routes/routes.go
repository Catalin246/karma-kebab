package routes

import (
	"event-service/handlers"
	"event-service/middlewares"
	"event-service/repositories"
	"event-service/services"
	"net/http"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/gorilla/mux"
	amqp "github.com/rabbitmq/amqp091-go"
)

func RegisterRoutes(serviceClient *aztables.ServiceClient, ch *amqp.Channel) *mux.Router {
	// Create the repository and service instances
	eventRepository := repositories.NewEventRepository(serviceClient)
	eventService := services.NewEventService(eventRepository)

	// Create the event handler and inject the service
	eventHandler := handlers.NewEventHandler(eventService, ch)

	r := mux.NewRouter()

	// Apply the middleware to all routes
	r.Use(middlewares.GatewayHeaderMiddleware)

	// Event routes
	r.HandleFunc("/events", eventHandler.GetEvents).Methods(http.MethodGet)
	r.HandleFunc("/events/{partitionKey}/{rowKey}", eventHandler.GetEventByID).Methods(http.MethodGet)
	r.HandleFunc("/events", eventHandler.CreateEvent).Methods(http.MethodPost)
	r.HandleFunc("/events/{partitionKey}/{rowKey}", eventHandler.UpdateEvent).Methods(http.MethodPut)
	r.HandleFunc("/events/{partitionKey}/{rowKey}", eventHandler.DeleteEvent).Methods(http.MethodDelete)

	return r
}
