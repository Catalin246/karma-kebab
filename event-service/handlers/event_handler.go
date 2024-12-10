package handlers

import (
	"context"
	"encoding/json"
	"event-service/models"
	"event-service/services"
	"log"
	"net/http"
	"time"

	"github.com/google/uuid"
	"github.com/gorilla/mux"
	amqp "github.com/rabbitmq/amqp091-go"
)

type EventHandler struct {
	service services.EventServiceInteface
	ch      *amqp.Channel
}

// NewEventHandler creates a new EventHandler
func NewEventHandler(service services.EventServiceInteface, ch *amqp.Channel) *EventHandler {
	return &EventHandler{service: service, ch: ch}
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

	// Declare the exchange
	q, err := h.ch.QueueDeclare(
		"eventCreated", // name
		false,          // durable
		false,          // delete when unused
		false,          // exclusive
		false,          // no-wait
		nil,            // arguments
	)
	if err != nil {
		http.Error(w, "Failed to declare a queue: "+err.Error(), http.StatusInternalServerError)
		return
	}

	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	body := "Event Created!"
	err = h.ch.PublishWithContext(ctx,
		"",     // exchange
		q.Name, // routing key
		false,  // mandatory
		false,  // immediate
		amqp.Publishing{
			ContentType: "text/plain",
			Body:        []byte(body),
		})
	//failOnError(err, "Failed to publish a message")
	log.Printf(" [x] Sent %s\n", body)

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
