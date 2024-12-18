package repositories

import (
	"context"
	"encoding/json"
	"fmt"
	"time"

	"github.com/Catalin246/karma-kebab/models"

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
	// Create clients for both the event table and the event shifts relationship table
	tableClient := r.serviceClient.NewClient(r.tableName)

	// Prepare the entity for the Event table insertion
	eventEntity := map[string]interface{}{
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

	// Marshal the event entity to JSON for storage
	eventEntityBytes, err := json.Marshal(eventEntity)
	if err != nil {
		return fmt.Errorf("failed to marshal event entity: %v", err)
	}

	// Insert the event into the main Event table
	_, err = tableClient.AddEntity(ctx, eventEntityBytes, nil)
	if err != nil {
		return fmt.Errorf("failed to insert event into Event table: %v", err)
	}

	return nil
}

// GetByID retrieves an event by PartitionKey and RowKey, including associated shift IDs
func (r *EventRepository) GetByID(ctx context.Context, partitionKey, rowKey string) (*models.Event, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)
	tableClientRelationship := r.serviceClient.NewClient("eventshifts")

	// Retrieve the event entity by PartitionKey and RowKey
	resp, err := tableClient.GetEntity(ctx, partitionKey, rowKey, nil)
	if err != nil {
		return nil, fmt.Errorf("failed to get event: %v", err)
	}

	// Parse the event data
	var eventData map[string]interface{}
	if err := json.Unmarshal(resp.Value, &eventData); err != nil {
		return nil, fmt.Errorf("failed to decode event: %v", err)
	}

	// Parse date fields
	startTime, err := time.Parse(time.RFC3339, eventData["StartTime"].(string))
	if err != nil {
		return nil, fmt.Errorf("failed to parse event start date: %v", err)
	}

	endTime, err := time.Parse(time.RFC3339, eventData["EndTime"].(string))
	if err != nil {
		return nil, fmt.Errorf("failed to parse event end date: %v", err)
	}

	// Parse RowKey as UUID
	rowKeyUUID, err := uuid.Parse(eventData["RowKey"].(string))
	if err != nil {
		return nil, fmt.Errorf("failed to parse RowKey as UUID: %v", err)
	}

	// Prepare the event object
	event := models.Event{
		PartitionKey: eventData["PartitionKey"].(string),
		RowKey:       rowKeyUUID,
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

	// Fetch the shift IDs associated with the event
	// The PartitionKey of the shift entities is the event's RowKey
	filter := fmt.Sprintf("PartitionKey eq '%s'", rowKey) // using the event's RowKey as PartitionKey for shifts
	listOptions := &aztables.ListEntitiesOptions{
		Filter: &filter,
	}
	pager := tableClientRelationship.NewListEntitiesPager(listOptions)

	var shiftIDs []string
	// Loop through pages of shift relationships
	for pager.More() {
		page, err := pager.NextPage(ctx)
		if err != nil {
			return nil, fmt.Errorf("failed to list shift relationships: %v", err)
		}

		// Unmarshal shift entities and collect ShiftIDs
		for _, entity := range page.Entities {
			var shiftData map[string]interface{}
			if err := json.Unmarshal(entity, &shiftData); err != nil {
				return nil, fmt.Errorf("failed to unmarshal shift relationship: %v", err)
			}

			shiftID := shiftData["RowKey"].(string) // The shift ID is in the RowKey
			shiftIDs = append(shiftIDs, shiftID)
		}
	}

	// Log the shiftIDs for debugging purposes
	fmt.Printf("Associated shift IDs: %v\n", shiftIDs)

	// Convert shift IDs to UUIDs and add them to the event object
	var shiftUUIDs []uuid.UUID
	for _, shiftID := range shiftIDs {
		shiftUUID, err := uuid.Parse(shiftID)
		if err != nil {
			return nil, fmt.Errorf("failed to parse shiftID as UUID: %v", err)
		}
		shiftUUIDs = append(shiftUUIDs, shiftUUID)
	}

	// Assign the parsed UUIDs to event.ShiftIDs
	event.ShiftIDs = shiftUUIDs

	return &event, nil
}

// GetAll retrieves all events, optionally filtered by date range
func (r *EventRepository) GetAll(ctx context.Context, filter string) ([]models.Event, error) {
	tableClient := r.serviceClient.NewClient(r.tableName)
	tableClientRelationship := r.serviceClient.NewClient("eventshifts")

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

			// Parse the date fields
			startTime, err := time.Parse(time.RFC3339, eventData["StartTime"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse event start date: %v", err)
			}

			endTime, err := time.Parse(time.RFC3339, eventData["EndTime"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse event end date: %v", err)
			}

			// Parse RowKey as UUID
			rowKeyUUID, err := uuid.Parse(eventData["RowKey"].(string))
			if err != nil {
				return nil, fmt.Errorf("failed to parse RowKey as UUID: %v", err)
			}

			// Prepare the event object
			event := models.Event{
				PartitionKey: eventData["PartitionKey"].(string),
				RowKey:       rowKeyUUID,
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

			// Fetch the shift IDs associated with the event (use RowKey of the event for PartitionKey in shifts)
			filterShifts := fmt.Sprintf("PartitionKey eq '%s'", rowKeyUUID.String()) // use event's RowKey as PartitionKey for shift relationships
			listOptionsShifts := &aztables.ListEntitiesOptions{
				Filter: &filterShifts,
			}
			pagerShifts := tableClientRelationship.NewListEntitiesPager(listOptionsShifts)

			var shiftIDs []string
			// Loop through pages of shift relationships
			for pagerShifts.More() {
				pageShifts, err := pagerShifts.NextPage(ctx)
				if err != nil {
					return nil, fmt.Errorf("failed to list shift relationships: %v", err)
				}

				// Unmarshal shift entities and collect ShiftIDs
				for _, entity := range pageShifts.Entities {
					var shiftData map[string]interface{}
					if err := json.Unmarshal(entity, &shiftData); err != nil {
						return nil, fmt.Errorf("failed to unmarshal shift relationship: %v", err)
					}

					shiftID := shiftData["RowKey"].(string) // The shift ID is in the RowKey
					shiftIDs = append(shiftIDs, shiftID)
				}
			}

			// Convert shift IDs to UUIDs and add them to the event object
			var shiftUUIDs []uuid.UUID
			for _, shiftID := range shiftIDs {
				shiftUUID, err := uuid.Parse(shiftID)
				if err != nil {
					return nil, fmt.Errorf("failed to parse shiftID as UUID: %v", err)
				}
				shiftUUIDs = append(shiftUUIDs, shiftUUID)
			}

			// Add the shift IDs to the event object
			event.ShiftIDs = shiftUUIDs

			// Append the event to the list of events
			events = append(events, event)
		}
	}

	return events, nil
}

// Update modifies an existing event
func (r *EventRepository) Update(ctx context.Context, partitionKey, rowKey string, event models.Event) error {
	tableClient := r.serviceClient.NewClient(r.tableName)
	tableClientRelationship := r.serviceClient.NewClient("eventshifts")

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

	// Marshal the updated event entity to JSON
	entityBytes, err := json.Marshal(entity)
	if err != nil {
		return fmt.Errorf("failed to marshal updated entity: %v", err)
	}

	// Update the event in the main event table
	_, err = tableClient.UpdateEntity(ctx, entityBytes, nil)
	if err != nil {
		return fmt.Errorf("failed to update event: %v", err)
	}

	// Now insert the new shift relationships for the updated event
	for _, shiftID := range event.ShiftIDs {
		shiftEntity := map[string]interface{}{
			"PartitionKey": partitionKey, // Event's partition key
			"EventRowKey":  rowKey,       // EventID
			"RowKey":       shiftID,      // ShiftID
		}

		// Marshal the shift entity to JSON
		shiftEntityBytes, err := json.Marshal(shiftEntity)
		if err != nil {
			return fmt.Errorf("failed to marshal shift entity: %v", err)
		}

		// Insert the new shift relationship into the eventshifts table
		_, err = tableClientRelationship.AddEntity(ctx, shiftEntityBytes, nil)
		if err != nil {
			return fmt.Errorf("failed to insert shift relationship into eventshifts table: %v", err)
		}
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
