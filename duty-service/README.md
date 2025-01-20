# Task - Duty Management Service

The **Duty Management Service** provides a structured way to manage and track tasks (duties) assigned to roles and shifts within the restaurant. It uses Azure Table Storage for data storage and retrieval.

---

## Models

### 1. **Duty**
Represents a specific task assigned to a role.

#### Fields:
- **`PartitionKey`** (string): Azure Table Storage PartitionKey.
- **`RowKey`** (UUID): Unique identifier for the task (Row Key).
- **`RoleId`** (int): ID of the associated role (enum in Employee MS).
- **`DutyName`** (string): Name of the duty.
- **`DutyDescription`** (string): Detailed description of the duty.

---

### 2. **DutyAssignment**
Represents a duty assigned to a specific employee for a specific shift.

#### Fields:
- **`PartitionKey`** (UUID): ShiftID (used as PartitionKey in Azure Table Storage).
- **`RowKey`** (UUID): DutyID (used as RowKey in Azure Table Storage).
- **`DutyAssignmentStatus`** (enum): Status of the duty assignment. Possible values:
  - `Completed`
  - `Incomplete`
- **`DutyAssignmentImageUrl`** (string, nullable): Optional URL to an image related to the duty.
- **`DutyAssignmentNote`** (string, nullable): Additional notes (optional).

---

### 3. **DutyAssignmentStatus**
Represents the status of a duty assignment.

#### Values:
- **`Completed`**: Duty is finished.
- **`Incomplete`**: Duty is not finished.

#### Validation:
Use the function **`ValidateDutyAssignmentStatus(status)`** to check if the status is valid.

---

## Endpoints

All routes are grouped under the base path `/duties`.

### Duty Management Endpoints
- **`GET /duties`**: Get all duties.
- **`GET /duties/{PartitionKey}/{RowKey}`**: Get details of a specific duty by its ID.
- **`GET /duties/role`**: Get duties associated with a specific role.
- **`POST /duties`**: Create a new duty (Admin role required).
- **`PUT /duties/{PartitionKey}/{RowKey}`**: Update an existing duty (Admin role required).
- **`DELETE /duties/{PartitionKey}/{RowKey}`**: Delete a duty (Admin role required).

### Duty Assignment Endpoints
- **`GET /duties/duty-assignments`**: Get all duty assignments for a specific shift.
- **`POST /duties/duty-assignments`**: Create new duty assignments.
- **`PUT /duties/duty-assignments/{ShiftId}/{DutyId}`**: Update a specific duty assignment.
- **`DELETE /duties/duty-assignments/{ShiftId}/{DutyId}`**: Delete a specific duty assignment.

### Metrics Endpoint
- **`GET /duties/metrics`**: Fetch Prometheus metrics for monitoring.



