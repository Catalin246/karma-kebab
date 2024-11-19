# Stage 1: Build the Go application
FROM golang:1.20 AS builder

WORKDIR /app

# Copy the Go module files and download dependencies
COPY go.mod go.sum ./
RUN go mod download

# Copy the rest of the application source code
COPY . .

# Build a statically linked binary
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -a -installsuffix cgo -o event-service .

# Stage 2: Use a minimal base image for running the application
FROM alpine:latest

RUN apk --no-cache add ca-certificates

WORKDIR /app

# Copy the statically built binary from the builder stage
COPY --from=builder /app/event-service .

# Ensure the binary is executable
RUN chmod +x /app/event-service

# Expose the application port
EXPOSE 3001

# Command to run the application
CMD ["./event-service"]
