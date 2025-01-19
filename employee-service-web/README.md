# Employee Service

The **Employee MicrosService** is a core component of the Karma Kebab project, responsible for managing employee-related operations such as adding, updating, and retrieving employee information. This service is built using C# and .NET and integrates with a repository to perform CRUD operations on employee data. It uses Entity Framework's Postgresql database.

## Features
- Add new employees with validation for roles and required fields.
- Update existing employee information with checks for invalid data.
- Retrieve employee details by role, id, or all.
- Integrate with Docker and OpenShift for scalable deployment.

## Table of Contents
- [Technologies Used](#technologies-used)
- [Prerequisites](#prerequisites)
- [Endpoints](#endpoints)
- [Development](#development)
- [Run Employee MS Locally](#run-employee-ms-locally)
- [Testing](#testing)

## Technologies Used
- **.NET Core**: Backend framework
- **C#**: Programming language
- **Postgresql**: Database
- **Entity Framework Core**: ORM for database operations
- **Moq** and **xUnit** for unit testing.

## Prerequisites
- [.NET SDK 6.0 or higher](https://dotnet.microsoft.com/download)
- A database (EntityFramework)
- IDE (e.g., Visual Studio or VS Code)

## Endpoints
Below are the primary endpoints exposed by the Employee Service:

### 1. Add Employee
- **Endpoint**: `POST /api/employees`
- **Description**: Adds a new employee to the system.
- **Request Body**:
    ```json
    {
        "firstName": "John",
        "lastName": "Doe",
        "email": "john.doe@example.com",
        "roles": [0],
        "payrate": 15.5,
        "dateOfBirth": "1990-05-15",
        "address": "123 Karma Street",
        "skills": [1, 2]
    }
    ```
- **Response**: Returns the created employee object.

### 2. Update Employee
- **Endpoint**: `PUT /api/employees/{employeeId}`
- **Description**: Updates information of an existing employee.
- **Request Body**: Similar to Add Employee.
- **Response**: Returns the updated employee object.

### 3. Get Employee by ID
- **Endpoint**: `GET /api/employees/{employeeId}`
- **Description**: Retrieves details of a specific employee.
- **Response**: Returns the employee object.

### 4. Get All Employees
- **Endpoint**: `GET /api/employees`
- **Description**: Retrieves a list of all employees.
- **Response**: Returns a list of employee objects.

## Development
Key classes and methods:

- **EmployeeController**: Handles API requests and routes.
- **EmployeeService**: Contains the business logic for employee operations.
- **EmployeeRepository**: Interacts with the database for CRUD operations.

## Local Employee Microservice project 

### Running Employee Web Api
1. Navigate to the `employee-service-web` directory:
    ```bash
    cd employee-service-web
    ```
2. Run Employee Microservice:
    ```bash
    dotnet run
    ```


## Testing
The project uses **Moq** and **xUnit** for unit testing. Tests are located in the `tests/unitTests` directory.

### Running Tests locally
1. Navigate to the `employee-service-web` directory:
    ```bash
    cd employee-service-web
    ```
2. Run tests:
    ```bash
    dotnet test
    ```
3. Ensure all tests pass before deployment.

### Common Test Cases
- Adding an employee with valid data.
- Attempting to add an employee with invalid roles.
- Updating an existing employee.
- Handling non-existent employees gracefully.
