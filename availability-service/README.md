# Availability Microservice

This microservice manages employee (un)availability for events in the Karma Kebab application.
each record will be a record of when an employee is NOT available

### Installation Steps

   "go mod init availability-microservice"
   "go get github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"
   "go get github.com/gin-gonic/gin"
   "go get github.com/spf13/viper"

### for local testing
   npm install -g azurite
   azurite

**Run the Service Locally**:

   go run main.go

   The service will be available at `http://localhost:8080`.

### Docker Setup (Optional)

1. **Build the Docker Image**:

   docker build -t availability-microservice .

2. **Run the Container**:

   docker run -p 8080:8080 availability-microservice

## API Documentation

### **POST** `/availability`

Create a new availability record for an employee.

#### Request Body:
```json
{
  "employeeID": "employee123",
  "startDate": "2024-12-01T09:00:00Z",
  "endDate": "2024-12-01T17:00:00Z"
}
```

#### Response:
- **201 Created**: Record created successfully.
- **400 Bad Request**: Invalid input data.

---

### **GET** `/availability/{employeeID}`

Get all availability records for a specific employee.

#### Response:
```json
[
  {
    "employeeID": "employee123",
    "startDate": "2024-12-01T09:00:00Z",
    "endDate": "2024-12-01T17:00:00Z"
  }
]
```

- **200 OK**: List of availability records for the employee.
- **404 Not Found**: Employee not found.

---

### **GET** `/availability/date/{date}`

Get all availability records for a specific date.

#### Response:
```json
[
  {
    "employeeID": "employee123",
    "startDate": "2024-12-01T09:00:00Z",
    "endDate": "2024-12-01T17:00:00Z"
  }
]
```

- **200 OK**: List of availability records for the date.
- **404 Not Found**: No availability found for the date.

---

### **PUT** `/availability/{id}`

Update an existing availability record.

#### Request Body:
```json
{
  "startDate": "2024-12-02T09:00:00Z",
  "endDate": "2024-12-02T17:00:00Z"
}
```

- **200 OK**: Record updated successfully.
- **404 Not Found**: Availability record not found.

---

### **DELETE** `/availability/{employeeID}/{id}`

Delete an availability record for an employee.

- **200 OK**: Record deleted successfully.
- **404 Not Found**: Record not found.

---