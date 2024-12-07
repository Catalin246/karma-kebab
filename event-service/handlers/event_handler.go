package handlers

import (
	"context"
	"encoding/json"
	"event-service/models"
	"event-service/services"
	"net/http"
	"time"

	"github.com/google/uuid"
	"github.com/gorilla/mux"
)

type EventHandler struct {
	service services.EventServiceInteface
}

// NewEventHandler creates a new EventHandler
func NewEventHandler(service services.EventServiceInteface) *EventHandler {
	return &EventHandler{service: service}
}

func (h *EventHandler) GetEvents(w http.ResponseWriter, r *http.Request) {
	query := r.URL.Query()
	startDate := query.Get("startDate")
	endDate := query.Get("endDate")

	var startTime, endTime *time.Time

	if startDate != "" {
		t, err := time.Parse(time.RFC3339, startDate)
		if err == nil {
			startTime = &t
		}
	}

	if endDate != "" {
		t, err := time.Parse(time.RFC3339, endDate)
		if err == nil {
			endTime = &t
		}
	}

	events, err := h.service.GetAll(context.Background(), startTime, endTime)
	if err != nil {
		http.Error(w, "Failed to retrieve events: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(events)
}

func (h *EventHandler) GetEventByID(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	partitionKey := vars["partitionKey"]
	rowKey := vars["rowKey"]

	event, err := h.service.GetByID(r.Context(), partitionKey, rowKey)
	if err != nil {
		if err.Error() == "event not found" {
			http.Error(w, `{"error": "event not found"}`, http.StatusNotFound)
		} else {
			http.Error(w, `{"error": "internal server error"}`, http.StatusInternalServerError)
		}
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(event)
}

func (h *EventHandler) CreateEvent(w http.ResponseWriter, r *http.Request) {
	var event models.Event
	if err := json.NewDecoder(r.Body).Decode(&event); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	event.RowKey = uuid.New()
	event.Date = time.Now()

	if err := h.service.Create(context.Background(), event); err != nil {
		http.Error(w, "Failed to create event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(map[string]string{"message": "Event created successfully"})
}

func (h *EventHandler) UpdateEvent(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	partitionKey := vars["partitionKey"]
	rowKey := vars["rowKey"]

	var event models.Event
	event.Date = time.Now()
	if err := json.NewDecoder(r.Body).Decode(&event); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	if err := h.service.Update(context.Background(), partitionKey, rowKey, event); err != nil {
		http.Error(w, "Failed to update event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "Event updated successfully"})
}

func (h *EventHandler) DeleteEvent(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	partitionKey := vars["partitionKey"]
	rowKey := vars["rowKey"]

	err := h.service.Delete(r.Context(), partitionKey, rowKey)
	if err != nil {
		if err.Error() == "event not found" {
			http.Error(w, `{"error": "event not found"}`, http.StatusNotFound)
		} else {
			http.Error(w, `{"error": "internal server error"}`, http.StatusInternalServerError)
		}
		return
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "Event deleted successfully"})
}
