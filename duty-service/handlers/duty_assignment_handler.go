package handlers

import (
	"context"
	"duty-service/models"
	"duty-service/services"
	"encoding/json"
	"fmt"
	"net/http"

	"github.com/google/uuid"
	"github.com/gorilla/mux"
)

type DutyAssignmentHandler struct {
	service services.InterfaceDutyAssignmentService
}

func NewDutyAssignmentHandler(service services.InterfaceDutyAssignmentService) *DutyAssignmentHandler {
	return &DutyAssignmentHandler{service: service}
}

// fetch all duty assignments by shiftId
func (h *DutyAssignmentHandler) GetAllDutyAssignmentsByShiftId(w http.ResponseWriter, r *http.Request) {
	query := r.URL.Query()
	shiftIdStr := query.Get("shiftId") // Get the shiftId parameter from the query string

	if shiftIdStr == "" { // check that the shiftId parameter is provided and is valid
		http.Error(w, "Missing 'shiftId' query parameter", http.StatusBadRequest)
		return
	}

	uuids, err := parseUUIDs(map[string]string{"shiftId": shiftIdStr})
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	// fetch duty assignments by shiftId via service
	dutyAssignments, err := h.service.GetAllDutyAssignmentsByShiftId(context.Background(), uuids["shiftId"])
	if err != nil {
		http.Error(w, "Failed to retrieve duty assignments: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	err = json.NewEncoder(w).Encode(dutyAssignments)
	if err != nil {
		http.Error(w, "Failed to encode response: "+err.Error(), http.StatusInternalServerError)
	}
}

// creates duty assignments for a Shift based on Role
func (h *DutyAssignmentHandler) CreateDutyAssignments(w http.ResponseWriter, r *http.Request) {
	var request struct {
		ShiftId string `json:"ShiftId"`
		RoleId  int    `json:"RoleId"`
	}

	if err := json.NewDecoder(r.Body).Decode(&request); err != nil {
		http.Error(w, "Invalid request body: "+err.Error(), http.StatusBadRequest)
		return
	}

	// change ShiftId as UUID
	shiftIdUUID, err := uuid.Parse(request.ShiftId)
	if err != nil {
		http.Error(w, "Invalid ShiftId format: "+err.Error(), http.StatusBadRequest)
		return
	}

	if err := h.service.CreateDutyAssignments(context.Background(), shiftIdUUID, request.RoleId); err != nil {
		http.Error(w, "Failed to create duty assignments: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusCreated)
	response := map[string]string{"message": "Duty assignments created successfully"}
	json.NewEncoder(w).Encode(response)
}

// updates a duty assignment //TODO make this method shorter
func (h *DutyAssignmentHandler) UpdateDutyAssignment(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)

	// get ShiftId and DutyId from path parameters
	// chekck if they are in the parameter
	if vars["ShiftId"] == "" || vars["DutyId"] == "" {
		http.Error(w, "Missing 'ShiftId' or 'DutyId' path parameter", http.StatusBadRequest)
		return
	}

	//parse the UUIDs
	uuids, err := parseUUIDs(map[string]string{"ShiftId": vars["ShiftId"], "DutyId": vars["DutyId"]})
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	// parsing the form to handle file upload (limit to 10MB)
	if err := r.ParseMultipartForm(10 << 20); err != nil {
		http.Error(w, "Failed to parse multipart form: "+err.Error(), http.StatusBadRequest)
		return
	}

	// get the file
	file, _, err := r.FormFile("image")
	if err != nil && err != http.ErrMissingFile {
		http.Error(w, "Failed to get image file: "+err.Error(), http.StatusBadRequest)
		return
	}
	defer func() {
		if file != nil {
			file.Close()
		}
	}()

	// get other form fields
	dutyAssignment := models.DutyAssignment{
		PartitionKey:         uuid.MustParse(r.FormValue("PartitionKey")),
		RowKey:               uuid.MustParse(r.FormValue("RowKey")),
		DutyAssignmentStatus: models.DutyAssignmentStatus(r.FormValue("DutyAssignmentStatus")),
	}

	// optional fields:
	if note := r.FormValue("DutyAssignmentNote"); note != "" {
		dutyAssignment.DutyAssignmentNote = &note
	}

	//check ShiftId and DutyId
	if dutyAssignment.PartitionKey != uuids["ShiftId"] || dutyAssignment.RowKey != uuids["DutyId"] {
		http.Error(w, "Mismatched ShiftId or DutyId in request body", http.StatusBadRequest)
		return
	}

	// check DutyAssignmentStatus
	if !models.ValidateDutyAssignmentStatus(dutyAssignment.DutyAssignmentStatus) {
		http.Error(w, "Invalid DutyAssignmentStatus. Valid values are 'Completed' or 'Incomplete'.", http.StatusBadRequest)
		return
	}

	// Call the service to update the duty assignment
	if err := h.service.UpdateDutyAssignment(context.Background(), dutyAssignment, file); err != nil {
		http.Error(w, "Failed to update duty assignment: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)                                                     // 200 ok
	response := map[string]string{"message": "Duty assignment updated successfully"} // if it is updated and the resposne is 200
	json.NewEncoder(w).Encode(response)
}

// deletes a duty assignment
func (h *DutyAssignmentHandler) DeleteDutyAssignment(w http.ResponseWriter, r *http.Request) {
	// get ShiftId and DutyId from path parameters
	vars := mux.Vars(r)

	if vars["ShiftId"] == "" || vars["DutyId"] == "" {
		http.Error(w, "Missing 'ShiftId' or 'DutyId' path parameter", http.StatusBadRequest)
		return
	}

	uuids, err := parseUUIDs(map[string]string{"ShiftId": vars["ShiftId"], "DutyId": vars["DutyId"]})
	if err != nil {
		http.Error(w, err.Error(), http.StatusBadRequest)
		return
	}

	if err := h.service.DeleteDutyAssignment(context.Background(), uuids["ShiftId"], uuids["DutyId"]); err != nil {
		http.Error(w, "Failed to delete duty assignment: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)                                                     // 200
	response := map[string]string{"message": "Duty assignment deleted successfully"} // if it is deleted and the resposne is 200
	json.NewEncoder(w).Encode(response)
}

// parses multiple UUID strings and returns them along with an error if any parsing fails.
func parseUUIDs(uuidStrings map[string]string) (map[string]uuid.UUID, error) {
	uuids := make(map[string]uuid.UUID)

	for key, value := range uuidStrings {
		parsedUUID, err := uuid.Parse(value)
		if err != nil {
			return nil, fmt.Errorf("invalid '%s' format: %v", key, err)
		}
		uuids[key] = parsedUUID
	}

	return uuids, nil
}
