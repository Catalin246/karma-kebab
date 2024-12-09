# RabbitMQ Service

This directory contains the necessary configuration for running a RabbitMQ service in a Docker container. The provided `Dockerfile` sets up RabbitMQ with the management console enabled.

## Features
- **RabbitMQ Version**: 3.x with the management plugin.
- **Exposed Ports**:
  - `5672`: RabbitMQ messaging port.
  - `15672`: RabbitMQ management console.


## Key points:

**IMessagePublisher** defines the contract for publishing messages
**IMessageConsumer** defines the contract for consuming messages
**RabbitMqConstants** provides centralized constants for queues and routing keys
**RabbitMqService** implements both interfaces with RabbitMQ-specific logic

The implementation includes:

Synchronous and asynchronous methods
Error handling
Logging (currently using Console)
Flexibility for different message types
Proper resource management with IDisposable