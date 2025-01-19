package services

import (
	"context"
	"encoding/json"
	"log"
	"time"

	"github.com/Catalin246/karma-kebab/models"
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
		"",           // exchange
		queueName,    // routing key (queue name)
		false,        // mandatory
		false,        // immediate
		amqp.Publishing{
			ContentType: "text/plain",
			Body:        []byte(message),
			DeliveryMode: amqp.Persistent,
			Timestamp:   time.Now(),
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
func (r *RabbitMQService) PublishEventDeleted(ctx context.Context, event *models.Event) error {
	shiftIDsBytes, err := json.Marshal(event.ShiftIDs)
	message := map[string]string{
		"eventID":     event.RowKey.String(),
		"shiftIDs": 	string(shiftIDsBytes),
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
		q.Name,    // queue
		"",        // consumer tag
		true,      // auto-acknowledge
		false,     // exclusive
		false,     // no-local
		false,     // no-wait
		nil,       // arguments
	)
	if err != nil {
		return err
	}

	// Start listening for messages
	go func() {
		for msg := range msgs {
			// Process message
			log.Printf("Received a message: %s", msg.Body)
			// Add message handling logic here, depending on the message type
		}
	}()

	return nil
}
