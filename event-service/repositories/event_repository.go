package repositories

import (
	"context"
	"encoding/json"
	"event-service/models"
	"fmt"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/google/uuid"
)

type EventRepository struct {
	serviceClient *aztables.ServiceClient
	tableName     string
}

// NewEventRepository creates a new EventRepository
func NewEventRepository(serviceClient *aztables.ServiceClient) *EventRepository {
	return &EventRepository{
		serviceClient: serviceClient,
		tableName:     "events",
	}
}

// Create inserts a new event into Azure Table Storage
func (r *EventRepository) Create(ctx context.Context, event models.Event) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Prepare the entity for insertion
	entity := map[string]interface{}{
		"PartitionKey": event.PartitionKey,
		"RowKey":       event.RowKey.String(), // convert UUID to string
		"StartTime":    event.StartTime.Format(time.RFC3339),
		"EndTime":      event.EndTime.Format(time.RFC3339),
		"Address":      event.Address,
		"Venue":        event.Venue,
		"Description":  event.Description,
		"Money":        event.Money,
		"Status":       string(event.Status),
		"FirstName":    event.Person.FirstName,
		"LastName":     event.Person.LastName,
		"Email":        event.Person.Email,
		"Note":         event.Note,
	}

	// Marshal the entity to JSON
	entityBytes, err := json.Marshal(entity)
	if err != nil {
		return fmt.Errorf("failed to marshal entity: %v", err)
	}

	// Insert the entity
	_, err = tableClient.AddEntity(ctx, entityBytes, nil)
	if err != nil {
		return fmt.Errorf("failed to insert event: %v", err)
	}

	return nil
}

// GetByID retrieves an event by PartitionKey and RowKey
func (r *EventRepository) GetByID(ctx context.Context, partitionKey, rowKey string) (*models.Event, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Retrieve the entity by PartitionKey and RowKey
	resp, err := tableClient.GetEntity(ctx, partitionKey, rowKey, nil)
	if err != nil {
		return nil, fmt.Errorf("failed to get event: %v", err)
	}

	// Parse the entity into a map
	var eventData map[string]interface{}
	if err := json.Unmarshal(resp.Value, &eventData); err != nil {
		return nil, fmt.Errorf("failed to decode event: %v", err)
	}

	// Parse date from string
	startTime, err := time.Parse(time.RFC3339, eventData["startTime"].(string))
	if err != nil {
		return nil, fmt.Errorf("failed to parse event date: %v", err)
	}

	endTime, err := time.Parse(time.RFC3339, eventData["endTime"].(string))
	if err != nil {
		return nil, fmt.Errorf("failed to parse event date: %v", err)
	}

	// Parse RowKey as UUID
	rowKeyUUID, err := uuid.Parse(eventData["RowKey"].(string))
	if err != nil {
		return nil, fmt.Errorf("failed to parse RowKey as UUID: %v", err)
	}

	// Return the event object
	event := models.Event{
		PartitionKey: eventData["PartitionKey"].(string),
		RowKey:       rowKeyUUID, // Assign UUID here
		StartTime:    startTime,
		EndTime:      endTime,
		Address:      eventData["Address"].(string),
		Venue:        eventData["Venue"].(string),
		Description:  eventData["Description"].(string),
		Money:        eventData["Money"].(float64),
		Status:       models.Status(eventData["Status"].(string)),
		Person: models.Person{
			FirstName: eventData["FirstName"].(string),
			LastName:  eventData["LastName"].(string),
			Email:     eventData["Email"].(string),
		},
		Note: eventData["Note"].(string),
	}

	return &event, nil
}

// GetAll retrieves all events, optionally filtered by date range
func (r *EventRepository) GetAll(ctx context.Context, filter string) ([]models.Event, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Create the query options with the filter
	listOptions := &aztables.ListEntitiesOptions{
		Filter: &filter,
	}

	pager := tableClient.NewListEntitiesPager(listOptions)
	var events []models.Event

	// Loop through pages of events
	for pager.More() {
		page, err := pager.NextPage(ctx)
		if err != nil {
			return nil, fmt.Errorf("failed to list events: %v", err)
		}

		// Unmarshal each entity and add to the events list
		for _, entity := range page.Entities {
			var eventData map[string]interface{}
			if err := json.Unmarshal(entity, &eventData); err != nil {
				return nil, fmt.Errorf("failed to unmarshal event: %v", err)
			}

			// Parse the date field
			startTime, err := time.Parse(time.RFC3339, eventData["startTime"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse event date: %v", err)
			}

			endTime, err := time.Parse(time.RFC3339, eventData["endTime"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse event date: %v", err)
			}

			// Parse RowKey as UUID
			rowKeyUUID, err := uuid.Parse(eventData["RowKey"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse RowKey as UUID: %v", err)
			}

			event := models.Event{
				PartitionKey: eventData["PartitionKey"].(string),
				RowKey:       rowKeyUUID, // Assign UUID here
				StartTime:    startTime,
				EndTime:      endTime,
				Address:      eventData["Address"].(string),
				Venue:        eventData["Venue"].(string),
				Description:  eventData["Description"].(string),
				Money:        eventData["Money"].(float64),
				Status:       models.Status(eventData["Status"].(string)),
				Person: models.Person{
					FirstName: eventData["FirstName"].(string),
					LastName:  eventData["LastName"].(string),
					Email:     eventData["Email"].(string),
				},
				Note: eventData["Note"].(string),
			}

			events = append(events, event)
		}
	}

	return events, nil
}

// Update modifies an existing event
func (r *EventRepository) Update(ctx context.Context, partitionKey, rowKey string, event models.Event) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Prepare the updated entity
	entity := map[string]interface{}{
		"PartitionKey": partitionKey,
		"RowKey":       rowKey,
		"StartTime":    event.StartTime.Format(time.RFC3339),
		"EndTime":      event.EndTime.Format(time.RFC3339),
		"Address":      event.Address,
		"Venue":        event.Venue,
		"Description":  event.Description,
		"Money":        event.Money,
		"Status":       string(event.Status),
		"FirstName":    event.Person.FirstName,
		"LastName":     event.Person.LastName,
		"Email":        event.Person.Email,
		"Note":         event.Note,
	}

	// Marshal the entity to JSON
	entityBytes, err := json.Marshal(entity)
	if err != nil {
		return fmt.Errorf("failed to marshal updated entity: %v", err)
	}

	// Update the entity
	_, err = tableClient.UpdateEntity(ctx, entityBytes, nil)
	if err != nil {
		return fmt.Errorf("failed to update event: %v", err)
	}

	return nil
}

// Delete removes an event by PartitionKey and RowKey
func (r *EventRepository) Delete(ctx context.Context, partitionKey, rowKey string) error {
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Delete the entity
	_, err := tableClient.DeleteEntity(ctx, partitionKey, rowKey, nil)
	if err != nil {
		return fmt.Errorf("failed to delete event: %v", err)
	}

	return nil
}
