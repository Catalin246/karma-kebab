package handlers

import (
	"context"
	"encoding/json"
	"event-service/db"
	"event-service/models"
	"log"
	"net/http"
	"time"

	"github.com/google/uuid"
	"github.com/gorilla/mux"
)

// GetEvents retrieves all events, optionally filtered by startDate and endDate
func GetEvents(w http.ResponseWriter, r *http.Request) {
	query := r.URL.Query()
	startDate := query.Get("startDate")
	endDate := query.Get("endDate")

	filter := ""
	if startDate != "" && endDate != "" {
		filter = "Date ge datetime'" + startDate + "' and Date le datetime'" + endDate + "'"
	}

	events, err := db.QueryTableWithFilter("events", filter)
	if err != nil {
		http.Error(w, "Failed to retrieve events: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(events)
}

// GetEventByID retrieves a specific event
func GetEventByID(w http.ResponseWriter, r *http.Request) {
	// Extract PartitionKey and RowKey from the URL path using mux.Vars
	vars := mux.Vars(r)
	partitionKey := vars["partitionKey"]
	rowKey := vars["rowKey"]

	log.Printf("Received Query Parameters - PartitionKey: %s, RowKey: %s", partitionKey, rowKey)

	if partitionKey == "" || rowKey == "" {
		log.Println("Missing PartitionKey or RowKey")
		http.Error(w, "Missing partitionKey or rowKey", http.StatusBadRequest)
		return
	}

	client, exists := db.TableClients["events"]
	if !exists {
		log.Println("Table client for 'events' not initialized")
		http.Error(w, "Table client not initialized", http.StatusInternalServerError)
		return
	}

	resp, err := client.GetEntity(context.Background(), partitionKey, rowKey, nil)
	if err != nil {
		log.Printf("Error retrieving entity - PartitionKey: %s, RowKey: %s, Error: %v", partitionKey, rowKey, err)
		http.Error(w, "Failed to retrieve event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	log.Printf("Raw Response from Azure Table Storage: %+v", resp)

	var event map[string]interface{}
	if err := json.Unmarshal(resp.Value, &event); err != nil {
		log.Printf("Error decoding response JSON: %v", err)
		http.Error(w, "Failed to decode event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	log.Printf("Final Response to Client: %+v", event)

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(event)
}

// CreateEvent creates a new event
func CreateEvent(w http.ResponseWriter, r *http.Request) {
	// Decode the request body into an Event object
	var event models.Event
	if err := json.NewDecoder(r.Body).Decode(&event); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	// Validate required fields
	if event.PartitionKey == "" || event.RowKey == uuid.Nil {
		http.Error(w, "Missing PartitionKey or RowKey in request body", http.StatusBadRequest)
		return
	}

	// Prepare the entity for Azure Table Storage
	entity := map[string]interface{}{
		"PartitionKey": event.PartitionKey,
		"RowKey":       event.RowKey,
		"Date":         event.Date.Format(time.RFC3339), // ISO 8601 format
		"Address":      event.Address,
		"Venue":        event.Venue,
		"Description":  event.Description,
		"Money":        event.Money,
		"Status":       string(event.Status),
		"FirstName":    event.Person.FirstName, // Flatten Person struct
		"LastName":     event.Person.LastName,
		"Email":        event.Person.Email,
		"Note":         event.Note,
	}

	// Marshal the entity map to JSON
	marshaledEntity, err := json.Marshal(entity)
	if err != nil {
		http.Error(w, "Failed to marshal event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Get the table client
	client, exists := db.TableClients["events"]
	if !exists {
		http.Error(w, "Table client not initialized", http.StatusInternalServerError)
		return
	}

	// Add the entity to Azure Table Storage
	_, err = client.AddEntity(context.Background(), marshaledEntity, nil)
	if err != nil {
		http.Error(w, "Failed to create event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Respond with success
	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(map[string]string{"message": "Event created successfully"})
}

// UpdateEvent updates an existing event
func UpdateEvent(w http.ResponseWriter, r *http.Request) {

}

// DeleteEvent deletes an existing event
func DeleteEvent(w http.ResponseWriter, r *http.Request) {

}
