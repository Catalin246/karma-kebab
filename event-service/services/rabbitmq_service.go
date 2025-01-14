package services

import (
	"context"
	"encoding/json"
	"log"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
	"github.com/Catalin246/karma-kebab/models"
	"github.com/Catalin246/karma-kebab/repositories"
	"github.com/google/uuid"
	amqp "github.com/rabbitmq/amqp091-go"
)

// RabbitMQService handles RabbitMQ messaging operations and implements RabbitMQServiceInterface
type RabbitMQService struct {
	Channel         *amqp.Channel
	EventRepository repositories.EventRepositoryInterface
}

// NewRabbitMQService initializes a new RabbitMQService
func NewRabbitMQService(ch *amqp.Channel, client *aztables.ServiceClient) *RabbitMQService {
	return &RabbitMQService{
		Channel:         ch,
		EventRepository: repositories.NewEventRepository(client),
	}
}

// PublishMessage publishes a message to the specified queue
func (r *RabbitMQService) PublishMessage(queueName, message string) error {
	// Declare the queue
	q, err := r.Channel.QueueDeclare(
		queueName, // name
		false,     // durable
		false,     // delete when unused
		false,     // exclusive
		false,     // no-wait
		nil,       // arguments
	)
	if err != nil {
		return err
	}

	// Set a timeout for publishing the message
	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()

	err = r.Channel.PublishWithContext(ctx,
		"",     // exchange
		q.Name, // routing key
		false,  // mandatory
		false,  // immediate
		amqp.Publishing{
			ContentType: "text/plain",
			Body:        []byte(message),
		})
	if err != nil {
		return err
	}

	log.Printf("[x] Sent %s\n", message)
	return nil
}

// ConsumeMessage consumes messages from the specified queue and processes them with the given handler function
func (r *RabbitMQService) ConsumeMessage(queueName string) error {
	// Declare the queue to ensure it exists before consuming
	q, err := r.Channel.QueueDeclare(
		queueName, // name
		false,     // durable
		false,     // delete when unused
		false,     // exclusive
		false,     // no-wait
		nil,       // arguments
	)
	if err != nil {
		return err
	}

	// Create a channel to receive messages
	msgs, err := r.Channel.Consume(
		q.Name, // queue
		"",     // consumer tag
		true,   // auto-ack
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
