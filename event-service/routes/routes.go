package routes

import (
	"event-service/handlers"
	"net/http"

	"github.com/gorilla/mux"
)

// Middleware for header validation
func gatewayHeaderMiddleware(next http.Handler) http.Handler {
	return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
		// Check for the presence and value of the custom header
		if r.Header.Get("X-From-Gateway") != "true" {
			http.Error(w, "Forbidden: Invalid Gateway Header", http.StatusForbidden)
			return
		}
		// Proceed to the next handler if header is valid
		next.ServeHTTP(w, r)
	})
}

func RegisterRoutes() *mux.Router {
	r := mux.NewRouter()

	// Apply the middleware to all routes
	r.Use(gatewayHeaderMiddleware)

	// Event routes
	r.HandleFunc("/events", handlers.GetEvents).Methods(http.MethodGet)
	r.HandleFunc("/events/{id}", handlers.GetEventByID).Methods(http.MethodGet)
	r.HandleFunc("/events", handlers.CreateEvent).Methods(http.MethodPost)
	r.HandleFunc("/events/{id}", handlers.UpdateEvent).Methods(http.MethodPut)
	r.HandleFunc("/events/{id}", handlers.DeleteEvent).Methods(http.MethodDelete)

	return r
}
