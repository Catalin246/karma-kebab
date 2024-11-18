# Use an official Golang image as the base image for building
FROM golang:1.18 AS builder

# Set the working directory in the container
WORKDIR /app

# Copy the go.mod and go.sum files and download dependencies
COPY go.mod go.sum ./
RUN go mod download

# Latest golang image on apline linux
FROM golang:1.17-alpine

# Work directory
WORKDIR /docker-go

# Installing dependencies
COPY go.mod go.sum ./
RUN go mod download

# Copying all the files
COPY . .

# Starting our application
CMD ["go", "run", "main.go"]

# Exposing server port
EXPOSE 5000

