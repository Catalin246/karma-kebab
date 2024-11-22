package routes

import (
	"event-service/handlers"
	"event-service/middlewares"
	"net/http"

	"github.com/gorilla/mux"
)

func RegisterRoutes() *mux.Router {
	r := mux.NewRouter()

	// Apply the middleware to all routes
	r.Use(middlewares.GatewayHeaderMiddleware)

	// Event routes
	r.HandleFunc("/events", handlers.GetEvents).Methods(http.MethodGet)
	r.HandleFunc("/events/{id}", handlers.GetEventByID).Methods(http.MethodGet)
	r.HandleFunc("/events", handlers.CreateEvent).Methods(http.MethodPost)
	r.HandleFunc("/events/{id}", handlers.UpdateEvent).Methods(http.MethodPut)
	r.HandleFunc("/events/{id}", handlers.DeleteEvent).Methods(http.MethodDelete)

	return r
}
