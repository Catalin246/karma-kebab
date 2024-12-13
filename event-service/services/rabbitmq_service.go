package services

import (
	"context"
	"log"
	"time"

	amqp "github.com/rabbitmq/amqp091-go"
)

// RabbitMQService handles RabbitMQ messaging operations
type RabbitMQService struct {
	Channel *amqp.Channel
}

// NewRabbitMQService initializes a new RabbitMQService
func NewRabbitMQService(ch *amqp.Channel) *RabbitMQService {
	return &RabbitMQService{Channel: ch}
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
