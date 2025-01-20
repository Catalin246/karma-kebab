# Karma Kebab Availability Microservice

## How to Use

### Requirements
- Go 1.18 or higher
- Azure Storage (for handling availability records)
- RabbitMQ (for message queue integration)
- Docker (for containerization)
- OpenShift (for deployment)

### Running Locally

To run the `Availability` microservice locally, follow these steps:

docker compose up on whole project
apigateway url/availability : localhost:3007/availability

### Endpoints

#### `GET /availability`
Retrieves all availability records.

- **Response:**
  - Status: `200 OK`
  - Body: JSON array of availability records.

#### `POST /availability`
Creates a new availability record for an employee.

- **Request Body:**
    ```json
    {
      "employeeId": "69ji0k34-k087-159j-fu3l-30718f822j436",
      "startDate": "2025-01-20T09:00:00Z",
      "endDate": "2025-01-20T17:00:00Z"
    }
    ```

- **Response:**
  - Status: `201 Created`
  - Body: The created availability record.

#### `PUT /availability/{partitionKey}/{rowKey}`
Updates an existing availability record by partition and row key.

- **Request Body:**
    ```json
    {
      "employeeId": "69ji0k34-k087-159j-fu3l-30718f822j436",
      "startDate": "2025-01-20T10:00:00Z",
      "endDate": "2025-01-20T18:00:00Z"
    }
    ```

- **Response:**
  - Status: `200 OK`
  - Body: The updated availability record.
  - Status: `404 Not Found` if the record does not exist.

#### `DELETE /availability/{partitionKey}/{rowKey}`
Deletes an availability record by partition and row key.

- **Response:**
  - Status: `204 No Content` if the record is deleted successfully.
  - Status: `404 Not Found` if the record does not exist.

#### `POST /availability/shift`
Receives a message about shift availability and checks for available employees.

- **Request Body:**
    ```json
    {
      "date": "2025-01-20T09:00:00Z",
      "employeeId": "69ji0k34-k087-159j-fu3l-30718f822j436"  // Optional employee ID filter
    }
    ```

- **Response:**
  - Status: `200 OK`
  - Body: JSON object containing available employee IDs for the requested shift date.

    ```json
    {
      "availableEmployeeIDs": ["u69ji0k34-k087-159j-fu3l-30718f822j436", "o39ji0k34-k087-159j-fu3l-30718f822j435"]
    }
    ```

### Message Queue Integration

The `Availability` microservice consumes and publishes messages via RabbitMQ. The service listens for messages regarding shift availability requests and publishes a response with available employees.

- **Consumption Queue:** `ShiftAvailabilityRequestQueue`
- **Response Queue:** `ShiftAvailabilityResponseQueue`

When a shift availability request is received, the microservice should check for available employees for the requested date and time, then responds with a list of available employees.

### Authentication
keycloak

### Storage

The microservice uses Azure Table Storage to store availability records