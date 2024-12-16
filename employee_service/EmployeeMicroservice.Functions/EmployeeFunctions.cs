using employee_service.Interfaces;
using employee_service.Repositories;
using employee_service.Services;
using employee_service.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using employee_service.utility;

namespace EmployeeMicroservice.Functions
{
    public class EmployeeFunctions
    {
        private readonly IEmployeeService _employeeService;

        // Constructor injection for EmployeeService
        public EmployeeFunctions(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // Get All Employees
        [Function("GetAllEmployees")]
        public async Task<HttpResponseData> GetAllEmployees(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "employees")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("GetAllEmployees");

            return await ExceptionService.HandleRequestAsync(async () =>
            {
                log.LogInformation("Fetching all employees");

                var employees = await _employeeService.GetAllEmployeesAsync();

                // Handle the empty case explicitly
                if (employees == null || !employees.Any())
                {
                    log.LogInformation("No employees found.");
                    var emptyResponse = req.CreateResponse(HttpStatusCode.OK);
                    await emptyResponse.WriteStringAsync("[]"); // Return an empty JSON array
                    return emptyResponse;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(JsonConvert.SerializeObject(employees));

                return response;
            }, log, req);
        }

        // Get Employee by ID
        [Function("GetEmployeeById")]
        public async Task<HttpResponseData> GetEmployeeById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "employees/{id:guid}")] HttpRequestData req,
            Guid id,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("GetEmployeeById");

            return await ExceptionService.HandleRequestAsync(async () =>
            {
                log.LogInformation($"Fetching employee with ID: {id}");

                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                var response = req.CreateResponse(employee != null ? HttpStatusCode.OK : HttpStatusCode.NotFound);

                if (employee != null)
                {
                    response.Headers.Add("Content-Type", "application/json");
                    await response.WriteStringAsync(JsonConvert.SerializeObject(employee));
                }

                return response;
            }, log, req);
        }

        // Get By Role
        [Function("GetEmployeeByRole")]
        public async Task<HttpResponseData> GetEmployeeByRole(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "employees/role/{role:int}")] HttpRequestData req,
            int role, // Change to accept a single int for role
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("GetEmployeeByRole");

            return await ExceptionService.HandleRequestAsync(async () =>
            {
                log.LogInformation($"Fetching employees with Role: {role}");

                // Validate role input as an enum
                if (!Enum.IsDefined(typeof(EmployeeRole), role))
                {
                    log.LogWarning("Invalid role specified.");
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Invalid role specified.");
                    return errorResponse;
                }

                var employeeRole = (EmployeeRole)role; // Convert int to the EmployeeRole enum

                // Fetch employees by role
                var employees = await _employeeService.GetEmployeesByRoleAsync(employeeRole); // Pass the enum value

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(JsonConvert.SerializeObject(employees));

                return response;
            }, log, req);
        }


        // Add Employee
        [Function("AddEmployee")]
        public async Task<HttpResponseData> AddEmployee(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "employees")] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = req.FunctionContext.GetLogger("AddEmployee");
            var log = executionContext.GetLogger("AddEmployee");

            return await ExceptionService.HandleRequestAsync(async () =>
            {
                log.LogInformation("Adding new employee");

                // Read body content of incoming Http request as string
                var content = await req.ReadAsStringAsync();
                var employeeDto = JsonConvert.DeserializeObject<EmployeeDTO>(content);

                logger.LogInformation($"Deserialized Employee: {JsonConvert.SerializeObject(employeeDto)}");

                var addedEmployee = await _employeeService.AddEmployeeAsync(employeeDto);

                var response = req.CreateResponse(HttpStatusCode.Created);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(JsonConvert.SerializeObject(addedEmployee));

                return response;
            }, log, req);
        }

        // Update Employee
        [Function("UpdateEmployee")]
        public async Task<HttpResponseData> UpdateEmployee(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "employees/{id:guid}")] HttpRequestData req,
            Guid id,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("UpdateEmployee");

            return await ExceptionService.HandleRequestAsync(async () =>
            {
                log.LogInformation($"Updating employee with ID: {id}");

                var requestBody = await req.ReadAsStringAsync();
                var updatedEmployeeDto = JsonConvert.DeserializeObject<EmployeeDTO>(requestBody);

                var updatedEmployee = await _employeeService.UpdateEmployeeAsync(id, updatedEmployeeDto);

                var response = req.CreateResponse(updatedEmployee != null ? HttpStatusCode.OK : HttpStatusCode.NotFound);

                if (updatedEmployee != null)
                {
                    response.Headers.Add("Content-Type", "application/json");
                    await response.WriteStringAsync(JsonConvert.SerializeObject(updatedEmployee));
                }

                return response;
            }, log, req);
        }

        // Delete Employee
        [Function("DeleteEmployee")]
        public async Task<HttpResponseData> DeleteEmployee(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "employees/{id:guid}")] HttpRequestData req,
            Guid id,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("DeleteEmployee");

            return await ExceptionService.HandleRequestAsync(async () =>
            {
                log.LogInformation($"Deleting employee with ID: {id}");

                var result = await _employeeService.DeleteEmployeeAsync(id);

                var response = req.CreateResponse(result ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
                return response;
            }, log, req);
        }
    }
}