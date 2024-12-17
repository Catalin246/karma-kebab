package services

import (
	"context"
	"duty-service/models"
	"encoding/json"
	"log"

	"github.com/streadway/amqp"
)

type RabbitMQService struct {
    connection *amqp.Connection
    channel    *amqp.Channel
    queueName  string
    exchange   string
}

func NewRabbitMQService(dutyService *DutyAssignmentService) *RabbitMQService {
    conn, err := amqp.Dial("amqp://karma-kebab-user:your-password@localhost:5672/karma-kebab")
    if err != nil {
        log.Fatal(err)
    }

    ch, err := conn.Channel()
    if err != nil {
        log.Fatal(err)
    }

    // Declare exchange
    err = ch.ExchangeDeclare(
        "clock-in-exchange", // name
        "topic",             // type
        true,                // durable
        false,               // auto-deleted
        false,               // internal
        false,               // no-wait
        nil,                 // arguments
    )

    // Declare queue
    q, err := ch.QueueDeclare(
        "clock-in-queue",    // name
        true,                // durable
        false,               // delete when unused
        false,               // exclusive
        false,               // no-wait
        nil,                 // arguments
    )

    // Bind queue to exchange
    err = ch.QueueBind(
        q.Name,              // queue name
        "employee.clock-in", // routing key
        "clock-in-exchange", // exchange
        false,
        nil,
    )

    // Start consuming messages
    msgs, err := ch.Consume(
        q.Name, // queue
        "",     // consumer
        false,  // auto-ack
        false,  // exclusive
        false,  // no-local
        false,  // no-wait
        nil,    // args
    )

    go func() {
        for msg := range msgs {
            var clockInMessage models.ClockInMessage
            err := json.Unmarshal(msg.Body, &clockInMessage)
            if err != nil {
                log.Printf("Error unmarshaling message: %v", err)
                msg.Nack(false, false)
                continue
            }

            // Create duty list based on clock-in message
            err = dutyService.CreateDutyAssignments(context.Background(), clockInMessage) //move to repo class?
            if err != nil {
                log.Printf("Error creating duty list: %v", err)
                msg.Nack(false, false)
                continue
            }

            // Acknowledge message
            msg.Ack(false)
        }
    }()

    return &RabbitMQService{
        connection: conn,
        channel:    ch,
        queueName:  q.Name,
        exchange:   "clock-in-exchange",
    }
}