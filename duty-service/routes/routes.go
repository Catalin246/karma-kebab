package routes

import (
	"duty-service/handlers"
	"duty-service/middlewares"
	"duty-service/repositories"
	"duty-service/services"
	"net/http"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/Azure/azure-sdk-for-go/sdk/storage/azblob"
	"github.com/gorilla/mux"
)

// sets up the router with all endpoints and middleware
func RegisterRoutes(serviceClient *aztables.ServiceClient, blobServiceClient *azblob.Client) *mux.Router {
	dutyRepository := repositories.NewDutyRepository(serviceClient)
	dutyService := services.NewDutyService(dutyRepository)

	dutyAssignmentRepository := repositories.NewDutyAssignmentRepository(serviceClient, blobServiceClient)
	dutyAssignmentService := services.NewDutyAssignmentService(dutyAssignmentRepository, dutyRepository)

	dutyHandler := handlers.NewDutyHandler(dutyService)
	dutyAssignmentHandler := handlers.NewDutyAssignmentHandler(dutyAssignmentService)

	r := mux.NewRouter() //main router

	r.Use(middlewares.GatewayHeaderMiddleware) // middleware for all routes routes to ensure requests come through the gateway

	dutiesRouter := r.PathPrefix("/duties").Subrouter() // group all routes under /duties

	dutiesRouter.Use(middlewares.JWTAuthMiddleware) // apply JWT authentication to all /duties routes

	// ---------------- DUTY ROUTES ----------------

	// Publicly accessible endpoint (no role requirement)
	dutiesRouter.HandleFunc("", dutyHandler.GetAllDuties).Methods(http.MethodGet)

	// Endpoint requiring the 'admin' role
	dutiesRouter.Handle("/role", middlewares.RoleMiddleware("admin", http.HandlerFunc(dutyHandler.GetDutiesByRole))).Methods(http.MethodGet)

	// Other duty routes with general authentication (no specific role requirement)
	dutiesRouter.HandleFunc("/{PartitionKey}/{RowKey}", dutyHandler.GetDutyById).Methods(http.MethodGet)
	dutiesRouter.HandleFunc("", dutyHandler.CreateDuty).Methods(http.MethodPost)
	dutiesRouter.HandleFunc("/{PartitionKey}/{RowKey}", dutyHandler.UpdateDuty).Methods(http.MethodPut)
	dutiesRouter.HandleFunc("/{PartitionKey}/{RowKey}", dutyHandler.DeleteDuty).Methods(http.MethodDelete)

	// ---------------- DUTY ASSIGNMENT ROUTES ----------------

	// Duty assignment routes with general authentication
	dutiesRouter.HandleFunc("/duty-assignments", dutyAssignmentHandler.GetAllDutyAssignmentsByShiftId).Methods(http.MethodGet)
	dutiesRouter.HandleFunc("/duty-assignments", dutyAssignmentHandler.CreateDutyAssignments).Methods(http.MethodPost)
	dutiesRouter.HandleFunc("/duty-assignments/{ShiftId}/{DutyId}", dutyAssignmentHandler.UpdateDutyAssignment).Methods(http.MethodPut)
	dutiesRouter.HandleFunc("/duty-assignments/{ShiftId}/{DutyId}", dutyAssignmentHandler.DeleteDutyAssignment).Methods(http.MethodDelete)

	return r
}
