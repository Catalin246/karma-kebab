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

	dutyAssignmentRepository := repositories.NewDutyAssignmentRepository(serviceClient)
	dutyAssignmentService := services.NewDutyAssignmentService(dutyAssignmentRepository)

	// Create the duty handler and inject the service
	dutyHandler := handlers.NewDutyHandler(dutyService)
	dutyAssignmentHandler := handlers.NewDutyAssignmentHandler(dutyAssignmentService)

	r := mux.NewRouter()

	// Apply the middleware to all routes
	r.Use(middlewares.GatewayHeaderMiddleware)

	// duty routes
	r.HandleFunc("/duties", dutyHandler.GetAllDuties).Methods(http.MethodGet)
	r.HandleFunc("/duties/{PartitionKey}/{RowKey}", dutyHandler.GetDutyById).Methods(http.MethodGet) //TODO: make partitionkey default "Duty"????? 27/11
	r.HandleFunc("/duties/role", dutyHandler.GetDutiesByRole).Methods(http.MethodGet)
	r.HandleFunc("/duties", dutyHandler.CreateDuty).Methods(http.MethodPost)
	r.HandleFunc("/duties/{PartitionKey}/{RowKey}", dutyHandler.UpdateDuty).Methods(http.MethodPut)    //TODO: make partitionkey default "Duty"????? 27/11
	r.HandleFunc("/duties/{PartitionKey}/{RowKey}", dutyHandler.DeleteDuty).Methods(http.MethodDelete) //TODO: make partitionkey default "Duty"????? 27/11

	// duty assignment routes:
	r.HandleFunc("/duty-assignments", dutyAssignmentHandler.GetAllDutyAssignmentsByShiftId).Methods(http.MethodGet) // example: http://localhost:5001/duty-assignments?shiftId=123e4567-e89b-12d3-a456-426614174000
	r.HandleFunc("/duty-assignments/{ShiftId}/{DutyId}", dutyAssignmentHandler.UpdateDutyAssignment).Methods(http.MethodPut)

	return r
}
