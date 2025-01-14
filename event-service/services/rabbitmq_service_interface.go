package services

// RabbitMQServiceInterface defines the interface for publishing messages
type RabbitMQServiceInterface interface {
	PublishMessage(queueName, message string) error
}
