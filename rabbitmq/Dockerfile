# RabbitMQ Dockerfile

# Use the official RabbitMQ image with management console
FROM rabbitmq:3-management

# Expose ports for RabbitMQ messaging and management console
EXPOSE 5672 15672

# Set default environment variables (optional, can override during runtime)
ENV RABBITMQ_DEFAULT_USER=admin
ENV RABBITMQ_DEFAULT_PASS=admin

# Optional: Add any custom configuration or plugins
# Example: Uncomment to add a custom config
# COPY rabbitmq.conf /etc/rabbitmq/

# Start RabbitMQ server
CMD ["rabbitmq-server"]
