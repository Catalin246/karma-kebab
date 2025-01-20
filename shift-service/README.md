
# Karma Kebab Shift Microservice

## How to Use

### Requirements
- .NET 8 
- Azure Storage (for handling storage operations)
- Docker (for containerization)
- OpenShift (for deployment)

### Running Locally

To run the `Shift` microservice locally, follow these steps:
- docker compose up on whole project
- api gateway url/shifts - localhost:3007/shifts

### Endpoints

#### `GET /shifts`
Retrieves a list of all shifts with optional filters:

- **Query Parameters:**
  - `startDate`: Start date for filtering (optional).
  - `endDate`: End date for filtering (optional).
  - `employeeId`: Filter by employee ID (optional).
  - `shiftType`: Filter by shift type (optional).
  - `shiftId`: Filter by shift ID (optional).
  - `eventId`: Filter by event ID (optional).

- **Response:**
  - Status: `200 OK`
  - Body: JSON array of shifts.

#### `GET /shifts/{shiftId:guid}`
Retrieves a specific shift by its ID.

- **Response:**
  - Status: `200 OK`
  - Body: JSON object containing the shift details.
  - Status: `404 Not Found` if the shift does not exist.

#### `POST /shifts`
Creates a new shift for an employee.

- **Request Body:**
    ```json
    {
      "employeeId": "uuid-5678",
      "startTime": "2025-01-20T09:00:00Z",
      "endTime": "2025-01-20T17:00:00Z",
      "shiftType": "Normal"
    }
    ```

- **Response:**
  - Status: `201 Created`
  - Body: The created shift.

#### `PUT /shifts/{shiftId:guid}`
Updates an existing shift by ID.

- **Request Body:**
    ```json
    {
      "employeeId": "uuid-5678",
      "startTime": "2025-01-20T10:00:00Z",
      "endTime": "2025-01-20T18:00:00Z",
      "shiftType": "Normal"
    }
    ```

- **Response:**
  - Status: `200 OK`
  - Body: The updated shift.
  - Status: `404 Not Found` if the shift does not exist.

#### `POST /shifts/{shiftId:guid}/clockin`
Clock in for a specific shift.

- **Response:**
  - Status: `200 OK`
  - Message: `"Clock-in successful and event published"` if the clock-in is successful and the event is published.
  - Status: `404 Not Found` if the shift does not exist.

#### `DELETE /shifts/{shiftId:guid}`
Deletes a shift by ID.

- **Response:**
  - Status: `204 No Content` if the shift is deleted successfully.
  - Status: `404 Not Found` if the shift does not exist.

#### `GET /shifts/{employeeId:guid}/hours`
Retrieves the total hours worked by an employee.

- **Response:**
  - Status: `200 OK`
  - Body: JSON object containing the total hours worked by the employee.
    ```json
    {
      "totalHours": 40
    }
    ```

### Authentication

Keycloak

### Event Publishing

Rabbit MQ
subs 
- event created
- event deleted

pubs
- clockin
- shift created
- shift deleted