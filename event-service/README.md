# event-service

## Set-up the project

1. Initialize the go.mod file

`go mod init event-service`

2. Import Packages  

`go get -u github.com/gorilla/mux`

3. Execute the project

`go run main.go` - this is not working. the connection string must be changed.

4. Execute unit tests

`go test ./tests/unit -v`


## RabbitMQ Service Functions
PublishEventCreated
Purpose: Publishes an event created message to RabbitMQ.
Routing Key: event.created
Message Body: Includes event details such as eventID, startTime, and endTime.
PublishEventUpdated
Purpose: Publishes an event updated message to RabbitMQ.
Routing Key: event.updated
Message Body: Includes updated event details like status, shiftIDs, and time range.
PublishEventDeleted
Purpose: Publishes an event deleted message to RabbitMQ.
Routing Key: event.deleted
Message Body: Includes the eventID and partitionKey to identify the deleted event.
ConsumeMessage
Purpose: Listens for and processes messages from RabbitMQ queues.
Message Handlers: Can be set up to consume specific types of event messages like shiftCreated.