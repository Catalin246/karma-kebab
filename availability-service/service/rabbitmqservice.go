// services/rabbitmq_service.go
package service

import (
	"log"

	amqp "github.com/rabbitmq/amqp091-go"
)

const (
	ShiftAvailabilityRequestQueue  = "shift-availability-request"
	ShiftAvailabilityResponseQueue = "shift-availability-response"
)

type RabbitMQService struct {
	channel *amqp.Channel
}

func NewRabbitMQService(ch *amqp.Channel) *RabbitMQService {
	return &RabbitMQService{
		channel: ch,
	}
}

func (s *RabbitMQService) SetupQueues() error {
	// Declare the request and response queues
	_, err := s.channel.QueueDeclare(
		ShiftAvailabilityRequestQueue,  // queue name
		true,   // durable
		false,  // delete when unused
		false,  // exclusive
		false,  // no-wait
		nil,    // arguments
	)
	if err != nil {
		return err
	}

	_, err = s.channel.QueueDeclare(
		ShiftAvailabilityResponseQueue,  // queue name
		true,   // durable
		false,  // delete when unused
		false,  // exclusive
		false,  // no-wait
		nil,    // arguments
	)
	return err
}

func (s *RabbitMQService) PublishMessage(queueName string, body []byte) error {
	return s.channel.Publish(
		"",        // exchange
		queueName, // routing key
		false,     // mandatory
		false,     // immediate
		amqp.Publishing{
			ContentType: "application/json",
			Body:        body,
		})
}

func (s *RabbitMQService) ConsumeMessages(queueName string, handler func([]byte) error) error {
	msgs, err := s.channel.Consume(
		queueName, // queue
		"",        // consumer
		true,      // auto-ack
		false,     // exclusive
		false,     // no-local
		false,     // no-wait
		nil,       // args
	)
	if err != nil {
		return err
	}

	go func() {
		for msg := range msgs {
			if err := handler(msg.Body); err != nil {
				log.Printf("Error processing message: %v", err)
			}
		}
	}()

	return nil
}