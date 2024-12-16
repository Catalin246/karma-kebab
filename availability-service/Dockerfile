# Stage 1: Build the Go application
FROM golang:1.20 AS builder

WORKDIR /app

# Copy the Go module files and download dependencies
COPY go.mod go.sum ./ 
RUN go mod download

# Copy the rest of the application source code
COPY . .

# Build a statically linked binary
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -a -installsuffix cgo -o availability-service .

# Stage 2: Use a minimal base image for running the application
FROM alpine:latest

RUN apk --no-cache add ca-certificates

WORKDIR /app

# Copy the binary and models
COPY --from=builder /app/availability-service .
COPY --from=builder /app/models ./models

# Ensure the binary is executable
RUN chmod +x /app/availability-service

# Expose the application port
EXPOSE 3002

# Command to run the application
CMD ["./availability-service"]


# // # Stop all running containers
# // docker-compose down

# // # Remove all containers
# // docker-compose rm -f

# // # Remove all images
# // docker-compose down --rmi all

# // # Remove all volumes
# // docker-compose down -v

# // # Rebuild with no cache
# // docker-compose build --no-cache

# // # Start containers
# // docker-compose up