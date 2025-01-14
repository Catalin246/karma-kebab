package services

import (
	"context"
	"encoding/json"
	"log"
	"time"

	"github.com/Catalin246/karma-kebab/models"
	"github.com/google/uuid"
	amqp "github.com/rabbitmq/amqp091-go"
)

// RabbitMQService handles RabbitMQ messaging operations
type RabbitMQService struct {
	channel *amqp.Channel
}

// NewRabbitMQService initializes a new RabbitMQService
func NewRabbitMQService(ch *amqp.Channel) *RabbitMQService {
	return &RabbitMQService{channel: ch}
}

// PublishMessage publishes a generic message to the RabbitMQ queue
func (r *RabbitMQService) PublishMessage(queueName, message string) error {
	return r.channel.Publish(
		"",        // exchange
		queueName, // routing key (queue name)
		false,     // mandatory
		false,     // immediate
		amqp.Publishing{
			ContentType:  "text/plain",
			Body:         []byte(message),
			DeliveryMode: amqp.Persistent,
			Timestamp:    time.Now(),
		},
	)
}

// PublishEventCreated publishes an event created message
func (r *RabbitMQService) PublishEventCreated(ctx context.Context, event models.Event) error {
	message := map[string]interface{}{
		"roleIDs":   event.RoleIDs,
		"eventID":   event.RowKey,
		"startTime": event.StartTime,
		"endTime":   event.EndTime,
	}

	messageBytes, err := json.Marshal(message)
	if err != nil {
		return err
	}

	return r.PublishMessage("event.created", string(messageBytes))
}

// PublishEventDeleted publishes an event deleted message
func (r *RabbitMQService) PublishEventDeleted(ctx context.Context, eventID string, partitionKey string) error {
	message := map[string]string{
		"eventID":      eventID,
		"partitionKey": partitionKey,
	}

	messageBytes, err := json.Marshal(message)
	if err != nil {
		return err
	}

	return r.PublishMessage("event.deleted", string(messageBytes))
}

// ConsumeMessage consumes messages from a specified queue - to add to in future
func (r *RabbitMQService) ConsumeMessage(queueName string) error {
	// Declare the queue (make sure it exists)
	q, err := r.channel.QueueDeclare(
		queueName, // Queue name
		true,      // durable
		false,     // auto-delete
		false,     // exclusive
		false,     // no-wait
		nil,       // arguments
	)
	if err != nil {
		return err
	}

	// Set up the consumer
	msgs, err := r.channel.Consume(
		q.Name, // queue
		"",     // consumer tag
		true,   // auto-acknowledge
		false,  // exclusive
		false,  // no-local
		false,  // no-wait
		nil,    // arguments
	)
	if err != nil {
		return err
	}

	// Consume messages asynchronously
	go func() {
		for msg := range msgs {
			// Deserialize message to extract required fields
			var payload struct {
				ShiftID      string `json:"ShiftId"`
				RowKey       string `json:"RowKey"`
				PartitionKey string `json:"PartitionKey"`
			}
			err := json.Unmarshal(msg.Body, &payload)
			if err != nil {
				log.Printf("[!] Error unmarshaling message: %v\n", err)
				continue
			}

			// Parse ShiftID
			parsedShiftID, err := uuid.Parse(payload.ShiftID)
			if err != nil {
				log.Printf("[!] Error parsing ShiftID: %v\n", err)
				continue
			}

			// Fetch the existing event using the repository
			existingEvent, err := r.EventRepository.GetByID(context.Background(), payload.PartitionKey, payload.RowKey)
			if err != nil {
				log.Printf("[!] Error fetching existing event: %v\n", err)
				continue
			}

			// Empty the list with shift ids in order to add only the new shifts
			existingEvent.ShiftIDs = nil

			// Create the updated event using old values where applicable
			updatedEvent := models.Event{
				PartitionKey: payload.PartitionKey,
				RowKey:       existingEvent.RowKey,      // Use the existing RowKey
				StartTime:    existingEvent.StartTime,   // Use the existing StartTime
				EndTime:      existingEvent.EndTime,     // Use the existing EndTime
				Address:      existingEvent.Address,     // Use the existing Address
				Venue:        existingEvent.Venue,       // Use the existing Venue
				Description:  existingEvent.Description, // Use the existing Description
				Money:        existingEvent.Money,       // Use the existing Money
				Status:       existingEvent.Status,      // Use the existing Status
				Person: models.Person{
					FirstName: existingEvent.Person.FirstName, // Use the existing first name
					LastName:  existingEvent.Person.LastName,  // Use the existing last name
					Email:     existingEvent.Person.Email,     // Use the existing email
				},
				Note:     existingEvent.Note,                            // Use the existing note
				ShiftIDs: append(existingEvent.ShiftIDs, parsedShiftID), // Append the new ShiftID
			}

			ctx := context.Background()
			err = r.EventRepository.Update(ctx, payload.PartitionKey, payload.RowKey, updatedEvent)
			if err != nil {
				log.Printf("[!] Error updating event: %v\n", err)
				continue
			}

			log.Printf("[x] Successfully updated event: %s\n", msg.Body)
		}
	}()

	return nil
}
