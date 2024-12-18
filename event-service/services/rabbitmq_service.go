package services

import (
	"context"
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

	shiftID := "da1d0a6d-d41d-482a-aec0-f239e3ad6b29"
	// Convert shiftID (string) to uuid.UUID
	parsedShiftID, err := uuid.Parse(shiftID)
	if err != nil {
		log.Printf("[!] Error parsing ShiftID: %v\n", err)
		return err
	}

	rowKey := "62ba06f4-fcbe-4a17-b196-a5348dc62d11"
	partitionKey := "event-group-winter"
	startTime := time.Date(2024, time.December, 18, 9, 0, 0, 0, time.UTC) // December 18, 2024 at 09:00 UTC
	endTime := time.Date(2024, time.December, 18, 17, 0, 0, 0, time.UTC)  // December 18, 2024 at 17:00 UTC

	// Assuming rowKey is a string, convert it to uuid.UUID
	parsedRowKey, err := uuid.Parse(rowKey) // Convert the string to uuid.UUID
	if err != nil {
		log.Printf("[!] Error parsing RowKey: %v\n", err)
		return err // Handle error accordingly
	}

	updatedEvent := models.Event{
		PartitionKey: partitionKey,               // Example partition key
		RowKey:       parsedRowKey,               // Use the UUID (RowKey) from the message
		StartTime:    startTime,                  // Use the parsed start time (e.g., from the message)
		EndTime:      endTime,                    // Use the parsed end time (e.g., from the message)
		Address:      "123 Event Street",         // Hardcoded address
		Venue:        "The Grand Hall",           // Hardcoded venue
		Description:  "Annual GoLang Conference", // Hardcoded description
		Money:        0.00,                       // Hardcoded money value
		Status:       models.Status("Cancelled"), // Hardcoded status (ensure it's one of the defined statuses)
		Person: models.Person{
			FirstName: "John",                // Hardcoded first name
			LastName:  "Doe",                 // Hardcoded last name
			Email:     "johndoe@example.com", // Hardcoded email
		},
		Note:     "Registration opens at 8:00 AM.", // Hardcoded note
		ShiftIDs: []uuid.UUID{parsedShiftID},       // Use the parsed shift ID (e.g., from the message)
	}

	ctx := context.Background()
	err = r.EventRepository.Update(ctx, partitionKey, rowKey, updatedEvent)

	// Consume messages asynchronously
	go func() {
		for msg := range msgs {
			if err != nil {
				log.Printf("[!] Error updating event: %v\n", err)
			} else {
				log.Printf("[x] Successfully updated event: %s\n", msg.Body)
			}
		}
	}()

	return nil
}
