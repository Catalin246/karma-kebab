package routes

import (
	"duty-service/handlers"
	"duty-service/middlewares"
	"duty-service/repositories"
	"duty-service/services"
	"net/http"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/gorilla/mux"
)

func RegisterRoutes(serviceClient *aztables.ServiceClient) *mux.Router {
	// Create the repository and service instances
	dutyRepository := repositories.NewDutyRepository(serviceClient)
	dutyService := services.NewDutyService(dutyRepository)

	// Create the duty handler and inject the service
	dutyHandler := handlers.NewDutyHandler(dutyService)

	r := mux.NewRouter()

	// Apply the middleware to all routes
	r.Use(middlewares.GatewayHeaderMiddleware)

	// duty routes
	r.HandleFunc("/duties", dutyHandler.GetAllDuties).Methods(http.MethodGet)

	return r
}
