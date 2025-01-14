package services

import (
	"context"

	"github.com/Catalin246/karma-kebab/models"
)

// RabbitMQServiceInterface defines the interface for publishing messages
type RabbitMQServiceInterface interface {
	PublishMessage(queueName, message string) error
	PublishEventCreated(ctx context.Context, event models.Event) error
	PublishEventDeleted(ctx context.Context, eventID string, partitionKey string) error
}
