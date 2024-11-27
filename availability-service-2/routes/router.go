package routes

import (
	"availability-service-2/handlers"
	"availability-service-2/middlewares"
	"availability-service-2/repository"
	"availability-service-2/service"
	"net/http"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/gorilla/mux"
)

func RegisterRoutes(serviceClient *aztables.ServiceClient) *mux.Router {
	// Create the repository and service instances
	availabilityRepository := repository.NewTableStorageAvailabilityRepository(serviceClient)
	availabilityService := service.NewAvailabilityService(availabilityRepository)

	// Create the availability handler and inject the service
	availabilityHandler := handlers.NewAvailabilityHandler(availabilityService)

	// Create a new Gorilla Mux router
	r := mux.NewRouter()

	// Apply middleware to all routes
	r.Use(middlewares.GatewayHeaderMiddleware)

	// Availability routes
	r.HandleFunc("/availability", availabilityHandler.GetAll).Methods(http.MethodGet)                         //works
	r.HandleFunc("/availability/{partitionKey}", availabilityHandler.GetByEmployeeID).Methods(http.MethodGet) //works
	r.HandleFunc("/availability", availabilityHandler.Create).Methods(http.MethodPost)                           //works
	r.HandleFunc("/availability/{partitionKey}/{rowKey}", availabilityHandler.Update).Methods(http.MethodPut)    //this gives me 'invaid id'
	r.HandleFunc("/availability/{partitionKey}/{rowKey}", availabilityHandler.Delete).Methods(http.MethodDelete) //'EmployeeID is required'
	http.Handle("/", r)
	return r
}

// # Stop all running containers
// docker-compose down

// # Remove all containers
// docker-compose rm -f

// # Remove all images
// docker-compose down --rmi all

// # Remove all volumes
// docker-compose down -v

// # Rebuild with no cache
// docker-compose build --no-cache

// # Start containers
// docker-compose up
