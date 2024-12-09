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
	dutyRepository := repositories.NewDutyRepository(serviceClient)
	dutyService := services.NewDutyService(dutyRepository)

	dutyAssignmentRepository := repositories.NewDutyAssignmentRepository(serviceClient)
	dutyAssignmentService := services.NewDutyAssignmentService(dutyAssignmentRepository, dutyRepository)

	dutyHandler := handlers.NewDutyHandler(dutyService)
	dutyAssignmentHandler := handlers.NewDutyAssignmentHandler(dutyAssignmentService)

	r := mux.NewRouter()

	// middleware for all routes
	r.Use(middlewares.GatewayHeaderMiddleware)

	// group all routes under /duties
	dutiesRouter := r.PathPrefix("/duties").Subrouter()

	// duty routes
	dutiesRouter.HandleFunc("", dutyHandler.GetAllDuties).Methods(http.MethodGet)
	dutiesRouter.HandleFunc("/{PartitionKey}/{RowKey}", dutyHandler.GetDutyById).Methods(http.MethodGet)
	dutiesRouter.HandleFunc("/role", dutyHandler.GetDutiesByRole).Methods(http.MethodGet)
	dutiesRouter.HandleFunc("", dutyHandler.CreateDuty).Methods(http.MethodPost)
	dutiesRouter.HandleFunc("/{PartitionKey}/{RowKey}", dutyHandler.UpdateDuty).Methods(http.MethodPut)
	dutiesRouter.HandleFunc("/{PartitionKey}/{RowKey}", dutyHandler.DeleteDuty).Methods(http.MethodDelete)

	// duty assignment routes (under /duties)
	dutiesRouter.HandleFunc("/duty-assignments", dutyAssignmentHandler.GetAllDutyAssignmentsByShiftId).Methods(http.MethodGet)
	dutiesRouter.HandleFunc("/duty-assignments", dutyAssignmentHandler.CreateDutyAssignments).Methods(http.MethodPost)
	dutiesRouter.HandleFunc("/duty-assignments/{ShiftId}/{DutyId}", dutyAssignmentHandler.UpdateDutyAssignment).Methods(http.MethodPut)
	dutiesRouter.HandleFunc("/duty-assignments/{ShiftId}/{DutyId}", dutyAssignmentHandler.DeleteDutyAssignment).Methods(http.MethodDelete)

	return r
}
