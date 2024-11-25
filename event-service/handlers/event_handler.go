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

	// Initialize the filter string
	var filter string

	// Filter based on startDate
	if startDate != "" {
		filter = "Date ge datetime'" + startDate + "'"
	}

	// Filter based on endDate
	if endDate != "" {
		if filter != "" {
			// If there's already a filter, append the endDate condition
			filter += " and Date le datetime'" + endDate + "'"
		} else {
			// If no startDate filter, just use the endDate filter
			filter = "Date le datetime'" + endDate + "'"
		}
	}

	// Log the filter query to verify correctness
	log.Printf("Filter query: %s", filter)

	// Execute the query with the filter
	events, err := db.QueryTableWithFilter("events", filter)
	if err != nil {
		http.Error(w, "Failed to retrieve events: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Send the response
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
	if event.PartitionKey == "" {
		http.Error(w, "Missing PartitionKey in request body", http.StatusBadRequest)
		return
	}

	// Auto-generate RowKey as a new UUID and set the Date to current time
	event.RowKey = uuid.New() // Generate a new UUID for RowKey
	event.Date = time.Now()   // Set the current time as Date

	// Prepare the entity for Azure Table Storage
	entity := map[string]interface{}{
		"PartitionKey": event.PartitionKey,
		"RowKey":       event.RowKey.String(),           // RowKey as string
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
	// Extract PartitionKey and RowKey from the URL path using mux.Vars
	vars := mux.Vars(r)
	partitionKey := vars["partitionKey"]
	rowKey := vars["rowKey"]

	log.Printf("Received Request to Update Event - PartitionKey: %s, RowKey: %s", partitionKey, rowKey)

	// Decode the request body into an Event object
	var event models.Event
	if err := json.NewDecoder(r.Body).Decode(&event); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	// Get the table client for "events"
	client, exists := db.TableClients["events"]
	if !exists {
		log.Println("Table client for 'events' not initialized")
		http.Error(w, "Table client not initialized", http.StatusInternalServerError)
		return
	}

	// Retrieve the existing entity from the table
	resp, err := client.GetEntity(context.Background(), partitionKey, rowKey, nil)
	if err != nil {
		log.Printf("Error retrieving entity - PartitionKey: %s, RowKey: %s, Error: %v", partitionKey, rowKey, err)
		http.Error(w, "Failed to retrieve event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Unmarshal the existing entity into a map
	var existingEvent map[string]interface{}
	if err := json.Unmarshal(resp.Value, &existingEvent); err != nil {
		log.Printf("Error decoding existing event: %v", err)
		http.Error(w, "Failed to decode existing event", http.StatusInternalServerError)
		return
	}

	event.Date = time.Now() // Set the current time as Date

	// Update the entity with the new values from the request body
	existingEvent["Date"] = event.Date.Format(time.RFC3339) // Update Date if provided
	existingEvent["Address"] = event.Address                // Update Address
	existingEvent["Venue"] = event.Venue                    // Update Venue
	existingEvent["Description"] = event.Description        // Update Description
	existingEvent["Money"] = event.Money                    // Update Money
	existingEvent["Status"] = string(event.Status)          // Update Status
	existingEvent["FirstName"] = event.Person.FirstName     // Update FirstName
	existingEvent["LastName"] = event.Person.LastName       // Update LastName
	existingEvent["Email"] = event.Person.Email             // Update Email
	existingEvent["Note"] = event.Note                      // Update Note

	// Marshal the updated entity back into JSON
	updatedEntity, err := json.Marshal(existingEvent)
	if err != nil {
		http.Error(w, "Failed to marshal updated event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Update the entity in Azure Table Storage
	_, err = client.UpdateEntity(context.Background(), updatedEntity, nil)
	if err != nil {
		log.Printf("Error updating entity - PartitionKey: %s, RowKey: %s, Error: %v", partitionKey, rowKey, err)
		http.Error(w, "Failed to update event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Respond with success
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "Event updated successfully"})
}

// DeleteEvent deletes an existing event
func DeleteEvent(w http.ResponseWriter, r *http.Request) {
	// Extract PartitionKey and RowKey from the URL path using mux.Vars
	vars := mux.Vars(r)
	partitionKey := vars["partitionKey"]
	rowKey := vars["rowKey"]

	log.Printf("Received Request to Delete Event - PartitionKey: %s, RowKey: %s", partitionKey, rowKey)

	// Get the table client for "events"
	client, exists := db.TableClients["events"]
	if !exists {
		log.Println("Table client for 'events' not initialized")
		http.Error(w, "Table client not initialized", http.StatusInternalServerError)
		return
	}

	// Delete the entity from Azure Table Storage
	_, err := client.DeleteEntity(context.Background(), partitionKey, rowKey, nil)
	if err != nil {
		log.Printf("Error deleting entity - PartitionKey: %s, RowKey: %s, Error: %v", partitionKey, rowKey, err)
		http.Error(w, "Failed to delete event: "+err.Error(), http.StatusInternalServerError)
		return
	}

	// Respond with success
	w.WriteHeader(http.StatusOK)
	json.NewEncoder(w).Encode(map[string]string{"message": "Event deleted successfully"})
}
