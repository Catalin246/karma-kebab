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
	r.HandleFunc("/availability", availabilityHandler.GetAll).Methods(http.MethodGet)
	r.HandleFunc("/availability/{partitionKey}", availabilityHandler.GetByEmployeeID).Methods(http.MethodGet)    // this one not reached?
	r.HandleFunc("/availability", availabilityHandler.Create).Methods(http.MethodPost)                           //this microservice reached but azurite not reached
	r.HandleFunc("/availability/{partitionKey}/{rowKey}", availabilityHandler.Update).Methods(http.MethodPut)    //this gives me 'invaid id'
	r.HandleFunc("/availability/{partitionKey}/{rowKey}", availabilityHandler.Delete).Methods(http.MethodDelete) //'EmployeeID is required'

	return r
}
