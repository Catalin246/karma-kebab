package services

import (
	"context"
	"duty-service/models"
	"encoding/json"
	"log"

	amqp "github.com/rabbitmq/amqp091-go"
)

type RabbitMQService struct {
	Connection  *amqp.Connection
	Channel     *amqp.Channel
	QueueName   string
	DutyService *DutyAssignmentService
}

func NewRabbitMQService(
	dutyService *DutyAssignmentService,
	connection *amqp.Connection,
) *RabbitMQService {
	// Create channel
	ch, err := connection.Channel()
	if err != nil {
		log.Fatalf("Failed to open a channel: %v", err)
	}

	// Declare queue
	q, err := ch.QueueDeclare(
		"clock-in", // queue name
		true,             // durable
		false,            // delete when unused
		false,            // exclusive
		false,            // no-wait
		nil,              // arguments
	)
	if err != nil {
		log.Fatalf("Failed to declare queue: %v", err)
	}

	rmqService := &RabbitMQService{
		Connection:  connection,
		Channel:     ch,
		QueueName:   q.Name,
		DutyService: dutyService,
	}

	// Start consuming messages
	rmqService.StartConsuming()

	return rmqService
}

func (s *RabbitMQService) StartConsuming() {
	// Consume messages
	msgs, err := s.Channel.Consume(
		s.QueueName, // queue
		"",          // consumer tag
		false,       // auto-ack
		false,       // exclusive
		false,       // no-local
		false,       // no-wait
		nil,         // arguments
	)
	if err != nil {
		log.Fatalf("Failed to register a consumer: %v", err)
	}

	go func() {
		for msg := range msgs {
			var clockInMessage models.ClockInMessage
			err := json.Unmarshal(msg.Body, &clockInMessage)
			if err != nil {
				log.Printf("Error unmarshaling message: %v", err)
				msg.Nack(false, false)
				continue
			}

			// Process the message
			err = s.DutyService.CreateDutyAssignments(context.Background(), clockInMessage.ShiftID, clockInMessage.RoleId)
			if err != nil {
				log.Printf("Error creating duty list: %v", err)
				msg.Nack(false, false)
				continue
			}

			// Acknowledge the message
			msg.Ack(false)
		}
	}()
}

// Close closes the RabbitMQ connection and channel
func (s *RabbitMQService) Close() {
	if s.Channel != nil {
		s.Channel.Close()
	}
	if s.Connection != nil {
		s.Connection.Close()
	}
}
