# shift-service
## Shifts Controller README

This document outlines the API endpoints provided by the `ShiftsController` class. The controller manages shift-related operations, including CRUD, clock-in/out, and retrieving employee hours.

---

### **Base URL**
`[controller]` is replaced by `shifts` in the actual endpoint URL.

---

### **Endpoints**

#### 1. **List Shifts**
- **Method**: `GET`
- **URL**: `/shifts`
- **Query Parameters** (optional):
  - `startDate` (DateTime): Filter shifts starting from this date.
  - `endDate` (DateTime): Filter shifts ending on this date.
  - `employeeId` (Guid): Filter shifts by employee ID.
  - `shiftType` (Enum): Filter shifts by type (e.g., Morning, Night).
  - `shiftId` (Guid): Retrieve a specific shift by ID.
  - `eventId` (Guid): Filter shifts by event ID.
- **Description**: Retrieves a list of shifts based on provided filters.

---

#### 2. **Get Shift by ID**
- **Method**: `GET`
- **URL**: `/shifts/{shiftId}`
- **Path Parameters**:
  - `shiftId` (Guid): The unique identifier of the shift.
- **Description**: Retrieves details of a specific shift.

---

#### 3. **Create a Shift**
- **Method**: `POST`
- **URL**: `/shifts`
- **Body Parameters**:
  - `CreateShiftDto` (JSON): Contains shift details like start time, end time, type, etc.
- **Description**: Creates a new shift.

---

#### 4. **Update a Shift**
- **Method**: `PUT`
- **URL**: `/shifts/{shiftId}`
- **Path Parameters**:
  - `shiftId` (Guid): The unique identifier of the shift to update.
- **Body Parameters**:
  - `UpdateShiftDto` (JSON): Contains updated shift details.
- **Description**: Updates an existing shift.

---

#### 5. **Clock In/Out**
- **Method**: `GET`
- **URL**: `/shifts/clock/{shiftId}`
- **Path Parameters**:
  - `shiftId` (Guid): The unique identifier of the shift to clock in or out.
- **Description**: Toggles the clock-in/out status of a shift. If a clock-in message is published to RabbitMQ.

---

#### 6. **Delete a Shift**
- **Method**: `DELETE`
- **URL**: `/shifts/{shiftId}`
- **Path Parameters**:
  - `shiftId` (Guid): The unique identifier of the shift to delete.
- **Description**: Deletes a shift.

---

#### 7. **Get Total Hours by Employee**
- **Method**: `GET`
- **URL**: `/shifts/{employeeId}/hours`
- **Path Parameters**:
  - `employeeId` (Guid): The unique identifier of the employee.
- **Description**: Retrieves the total hours worked by the specified employee.

---

### **Response Format**

All endpoints return the following response structure:
```json
{
    "success": true,
    "message": "Description of the outcome",
    "data": { }
}
```

### **Error Responses**
- **404 Not Found**: Returned when the requested resource is not found.
- **500 Internal Server Error**: Returned when an unexpected error occurs.
- **400 Bad Request**: Returned when input data is invalid.

---

### **Models**

#### **CreateShiftDto**
```json
{
    "startTime": "DateTime",
    "endTime": "DateTime",
    "shiftType": "string",
    "status": "string",
    "roleId": "int"
}
```

#### **UpdateShiftDto**
```json
{
    "startTime": "DateTime",
    "endTime": "DateTime",
    "shiftType": "string",
    "status": "string",
    "clockInTime": "DateTime?",
    "clockOutTime": "DateTime?",
    "roleId": "int"
}
```

---

Let me know if you need further clarification or adjustments!