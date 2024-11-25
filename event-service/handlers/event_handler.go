package handlers

import (
	"context"
	"encoding/json"
	"event-service/db"
	"net/http"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
)

// GetEvents retrieves all events, optionally filtered by startDate and endDate
func GetEvents(w http.ResponseWriter, r *http.Request) {
	query := r.URL.Query()
	startDate := query.Get("startDate")
	endDate := query.Get("endDate")

	filter := ""
	if startDate != "" && endDate != "" {
		filter = "Timestamp ge datetime'" + startDate + "' and Timestamp le datetime'" + endDate + "'"
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
	query := r.URL.Query()
	partitionKey := query.Get("partitionKey")
	rowKey := query.Get("rowKey")

	if partitionKey == "" || rowKey == "" {
		http.Error(w, "Missing partitionKey or rowKey", http.StatusBadRequest)
		return
	}

	client, exists := db.TableClients["events"]
	if !exists {
		http.Error(w, "Table client not initialized", http.StatusInternalServerError)
		return
	}

	resp, err := client.GetEntity(context.Background(), partitionKey, rowKey, nil)
	if err != nil {
		http.Error(w, "Failed to retrieve event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	var event map[string]interface{}
	if err := json.Unmarshal(resp.Value, &event); err != nil {
		http.Error(w, "Failed to decode event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(event)
}

// CreateEvent creates a new event
func CreateEvent(w http.ResponseWriter, r *http.Request) {
	var event map[string]interface{}
	if err := json.NewDecoder(r.Body).Decode(&event); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	client, exists := db.TableClients["events"]
	if !exists {
		http.Error(w, "Table client not initialized", http.StatusInternalServerError)
		return
	}

	// Ensure PartitionKey and RowKey are part of the request
	if _, ok := event["PartitionKey"]; !ok {
		http.Error(w, "Missing PartitionKey in request body", http.StatusBadRequest)
		return
	}
	if _, ok := event["RowKey"]; !ok {
		http.Error(w, "Missing RowKey in request body", http.StatusBadRequest)
		return
	}

	// Marshal the event for Azure Table Storage
	marshaledEvent, err := json.Marshal(event)
	if err != nil {
		http.Error(w, "Failed to marshal event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Insert the entity into the table
	_, err = client.AddEntity(context.Background(), marshaledEvent, nil)
	if err != nil {
		http.Error(w, "Failed to create event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusCreated)
	json.NewEncoder(w).Encode(map[string]string{"message": "Event created"})
}

// UpdateEvent updates an existing event
func UpdateEvent(w http.ResponseWriter, r *http.Request) {
	var event map[string]interface{}
	if err := json.NewDecoder(r.Body).Decode(&event); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	client, exists := db.TableClients["events"]
	if !exists {
		http.Error(w, "Table client not initialized", http.StatusInternalServerError)
		return
	}

	// Ensure PartitionKey and RowKey are part of the request
	partitionKey, ok := event["PartitionKey"].(string)
	if !ok || partitionKey == "" {
		http.Error(w, "Missing or invalid PartitionKey in request body", http.StatusBadRequest)
		return
	}
	rowKey, ok := event["RowKey"].(string)
	if !ok || rowKey == "" {
		http.Error(w, "Missing or invalid RowKey in request body", http.StatusBadRequest)
		return
	}

	// Marshal the event for Azure Table Storage
	marshaledEvent, err := json.Marshal(event)
	if err != nil {
		http.Error(w, "Failed to marshal event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Update the entity in the table
	_, err = client.UpsertEntity(context.Background(), marshaledEvent, aztables.UpdateModeReplace, UpsertEntityOptions{})
	if err != nil {
		http.Error(w, "Failed to update event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "Event updated"})
}

// DeleteEvent deletes an existing event
func DeleteEvent(w http.ResponseWriter, r *http.Request) {
	query := r.URL.Query()
	partitionKey := query.Get("partitionKey")
	rowKey := query.Get("rowKey")

	if partitionKey == "" || rowKey == "" {
		http.Error(w, "Missing partitionKey or rowKey", http.StatusBadRequest)
		return
	}

	client, exists := db.TableClients["events"]
	if !exists {
		http.Error(w, "Table client not initialized", http.StatusInternalServerError)
		return
	}

	_, err := client.DeleteEntity(context.Background(), partitionKey, rowKey, nil)
	if err != nil {
		http.Error(w, "Failed to delete event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "Event deleted"})
}
