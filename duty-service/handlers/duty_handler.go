package handlers

import (
	"context"
	"duty-service/services"
	"encoding/json"
	"net/http"
)

type DutyHandler struct {
	service services.InterfaceDutyService
}

// NewDutyHandler creates a new DutyHandler
func NewDutyHandler(service services.InterfaceDutyService) *DutyHandler {
	return &DutyHandler{service: service}
}

func (h *DutyHandler) GetAllDuties(w http.ResponseWriter, r *http.Request) {
	query := r.URL.Query()
	name := query.Get("name") // Get the name parameter from the query string

	duties, err := h.service.GetAllDuties(context.Background(), name)
	if err != nil {
		http.Error(w, "Failed to retrieve duties: "+err.Error(), http.StatusInternalServerError)
		return
	}
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(duties)
}
