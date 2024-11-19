# event-service

## Set-up the project

`go mod init event-service`

`go get -u github.com/gorilla/mux`

`go run main.go` - this is not working. the connection string must be changed.

## Project structure

event-service/
├── go.mod                        # Go module file
├── go.sum                        # Dependency file
├── .env                          # Environment variables (e.g., Azure keys, Keycloak info)
├── .gitignore                    # Git ignore file (exclude .env and other sensitive files)
├── main.go                       # Entry point of the application
├── models/                       # Directory for all models
│   ├── event.go                  # Event model
│   ├── person.go                 # Person model
├── db/                           # Database-related logic
│   └── azure_table.go            # Azure Table Storage client
├── middleware/                   # Middleware for the app
│   └── auth.go                   # Authentication middleware using Keycloak
├── handlers/                     # Handlers for API endpoints
│   └── event_handler.go          # Event-specific handlers (CRUD operations)
├── routes/                       # Directory for routes
│   └── routes.go                 # Route definitions
├── tests/                        # Directory for tests
│   ├── unit/                     # Unit tests
│   │   └── event_handler_test.go # Unit tests for event handlers
│   ├── integration/              # Integration tests
│   │   └── integration_test.go   # Integration tests for the API
└── README.md                     # Documentation for the project