package services

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"time"

	"github.com/Catalin246/karma-kebab/models"
	"github.com/google/uuid"
	amqp "github.com/rabbitmq/amqp091-go"
)

const (
	ShiftServiceEventCreatedQueue = "shift-service.event.created"
	ShiftServiceEventDeletedQueue = "shift-service.event.deleted"
)

// EventCreatedMessage represents the structure of the event created message
type EventCreatedMessage struct {
	RoleIDs   []int  `json:"roleIds"`
	EventID   string `json:"eventId"` // UUID will be converted to string
	StartTime string `json:"startTime"`
	EndTime   string `json:"endTime"`
}

// EventDeletedMessage represents the structure of the event deleted message
type EventDeletedMessage struct {
	ShiftIDs []uuid.UUID `json:"shiftIds"`
	EventID  string      `json:"eventId"` // UUID will be converted to string
}

// RabbitMQService handles RabbitMQ messaging operations
type RabbitMQService struct {
	channel *amqp.Channel
}

// NewRabbitMQService initializes a new RabbitMQService
func NewRabbitMQService(ch *amqp.Channel) (*RabbitMQService, error) {
	service := &RabbitMQService{channel: ch}
	if err := service.initializeQueues(); err != nil {
		return nil, fmt.Errorf("failed to initialize queues: %w", err)
	}
	return service, nil
}

// initializeQueues declares all required queues
func (r *RabbitMQService) initializeQueues() error {
	// Declare event.created queue
	_, err := r.channel.QueueDeclare(
		ShiftServiceEventCreatedQueue,
		true,  // durable
		false, // auto-delete
		false, // exclusive
		false, // no-wait
		nil,   // arguments
	)
	if err != nil {
		return fmt.Errorf("failed to declare event.created queue: %w", err)
	}

	// Declare event.deleted queue
	_, err = r.channel.QueueDeclare(
		ShiftServiceEventDeletedQueue,
		true,  // durable
		false, // auto-delete
		false, // exclusive
		false, // no-wait
		nil,   // arguments
	)
	if err != nil {
		return fmt.Errorf("failed to declare event.deleted queue: %w", err)
	}

	return nil
}

// PublishMessage publishes a generic message to the RabbitMQ queue
func (r *RabbitMQService) PublishMessage(ctx context.Context, queueName string, message []byte) error {
	return r.channel.PublishWithContext(
		ctx,
		"",        // exchange
		queueName, // routing key (queue name)
		true,      // mandatory
		false,     // immediate
		amqp.Publishing{
			ContentType:  "application/json",
			Body:        message,
			DeliveryMode: amqp.Persistent,
			Timestamp:   time.Now(),
		},
	)
}

// PublishEventCreated publishes an event created message
func (r *RabbitMQService) PublishEventCreated(ctx context.Context, event models.Event) error {
	message := EventCreatedMessage{
		RoleIDs:   event.RoleIDs,
		EventID:   event.RowKey.String(), //stores as string in db
		StartTime: event.StartTime.String(),
		EndTime:   event.EndTime.String(),
	}

	messageBytes, err := json.Marshal(message)
	if err != nil {
		return fmt.Errorf("failed to marshal event created message: %w", err)
	}

	if err := r.PublishMessage(ctx, ShiftServiceEventCreatedQueue, messageBytes); err != nil {
		return fmt.Errorf("failed to publish event created message: %w", err)
	}

	log.Printf("Published event created message for event %s", event.RowKey.String())
	return nil
}

// PublishEventDeleted publishes an event deleted message
func (r *RabbitMQService) PublishEventDeleted(ctx context.Context, eventID uuid.UUID, shiftIDs []uuid.UUID) error {
	message := EventDeletedMessage{
		EventID:  eventID.String(), // Convert UUID to string
		ShiftIDs: shiftIDs,
	}

	messageBytes, err := json.Marshal(message)
	if err != nil {
		return fmt.Errorf("failed to marshal event deleted message: %w", err)
	}

	if err := r.PublishMessage(ctx, ShiftServiceEventDeletedQueue, messageBytes); err != nil {
		return fmt.Errorf("failed to publish event deleted message: %w", err)
	}

	log.Printf("Published event deleted message for event %s", eventID.String())
	return nil
}

// ConsumeMessages sets up message consumption for all queues
func (r *RabbitMQService) ConsumeMessages(ctx context.Context) error {
	// Set up event.created consumer
	createdMsgs, err := r.channel.Consume(
		ShiftServiceEventCreatedQueue,
		"",    // consumer tag
		false, // auto-ack
		false, // exclusive
		false, // no-local
		false, // no-wait
		nil,   // arguments
	)
	if err != nil {
		return fmt.Errorf("failed to set up event.created consumer: %w", err)
	}

	// Set up event.deleted consumer
	deletedMsgs, err := r.channel.Consume(
		ShiftServiceEventDeletedQueue,
		"",    // consumer tag
		false, // auto-ack
		false, // exclusive
		false, // no-local
		false, // no-wait
		nil,   // arguments
	)
	if err != nil {
		return fmt.Errorf("failed to set up event.deleted consumer: %w", err)
	}

	// Handle messages in separate goroutines
	go r.handleEventCreatedMessages(ctx, createdMsgs)
	go r.handleEventDeletedMessages(ctx, deletedMsgs)

	return nil
}

func (r *RabbitMQService) handleEventCreatedMessages(ctx context.Context, msgs <-chan amqp.Delivery) {
	for msg := range msgs {
		var eventMessage EventCreatedMessage
		if err := json.Unmarshal(msg.Body, &eventMessage); err != nil {
			log.Printf("Error unmarshaling event created message: %v", err)
			msg.Nack(false, true) // Negative acknowledgment, requeue
			continue
		}

		// Process the message
		log.Printf("Processing event created message for event %s", eventMessage.EventID)
		
		msg.Ack(false) // Acknowledge the message
	}
}

func (r *RabbitMQService) handleEventDeletedMessages(ctx context.Context, msgs <-chan amqp.Delivery) {
	for msg := range msgs {
		var eventMessage EventDeletedMessage
		if err := json.Unmarshal(msg.Body, &eventMessage); err != nil {
			log.Printf("Error unmarshaling event deleted message: %v", err)
			msg.Nack(false, true) // Negative acknowledgment, requeue
			continue
		}

		// Process the message
		log.Printf("Processing event deleted message for event %s", eventMessage.EventID)
		
		msg.Ack(false) // Acknowledge the message
	}
}