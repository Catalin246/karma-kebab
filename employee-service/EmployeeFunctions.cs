using Interfaces;
using Models;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace employee_service
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
                try
                {
                    log.LogInformation("Fetching all employees");
                    var employees = await _employeeService.GetAllEmployeesAsync();

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
                }
                catch (Exception ex)
                {
                    log.LogError($"Error: {ex.Message}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync($"An error occurred while fetching employees: {ex.Message}");
                    return errorResponse;
                }
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
                try
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
                }
                catch (Exception ex)
                {
                    log.LogError($"Error: {ex.Message}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync($"An error occurred while fetching the employee: {ex.Message}");
                    return errorResponse;
                }
            }, log, req);
        }

        // Get By Role
        [Function("GetEmployeeByRole")]
        public async Task<HttpResponseData> GetEmployeeByRole(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "employees/role/{role:int}")] HttpRequestData req,
            int role,
            FunctionContext executionContext)
        {
            var log = executionContext.GetLogger("GetEmployeeByRole");

            return await ExceptionService.HandleRequestAsync(async () =>
            {
                try
                {
                    log.LogInformation($"Fetching employees with Role: {role}");

                    if (!Enum.IsDefined(typeof(EmployeeRole), role))
                    {
                        log.LogWarning("Invalid role specified.");
                        var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                        await errorResponse.WriteStringAsync("Invalid role specified.");
                        return errorResponse;
                    }

                    var employeeRole = (EmployeeRole)role;
                    var employees = await _employeeService.GetEmployeesByRoleAsync(employeeRole);

                    var response = req.CreateResponse(HttpStatusCode.OK);
                    response.Headers.Add("Content-Type", "application/json");
                    await response.WriteStringAsync(JsonConvert.SerializeObject(employees));

                    return response;
                }
                catch (Exception ex)
                {
                    log.LogError($"Error: {ex.Message}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync($"An error occurred while fetching employees by role: {ex.Message}");
                    return errorResponse;
                }
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
                try
                {
                    log.LogInformation("Adding new employee");

                    var content = await req.ReadAsStringAsync();
                    var employeeDto = JsonConvert.DeserializeObject<EmployeeDTO>(content);

                    if (employeeDto == null)
                    {
                        log.LogError("Invalid employee data.");
                        var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                        await errorResponse.WriteStringAsync("Invalid employee data provided.");
                        return errorResponse;
                    }

                    logger.LogInformation($"Deserialized Employee: {JsonConvert.SerializeObject(employeeDto)}");

                    var addedEmployee = await _employeeService.AddEmployeeAsync(employeeDto);

                    var response = req.CreateResponse(HttpStatusCode.Created);
                    response.Headers.Add("Content-Type", "application/json");
                    await response.WriteStringAsync(JsonConvert.SerializeObject(addedEmployee));

                    return response;
                }
                catch (Exception ex)
                {
                    log.LogError($"Error: {ex.Message}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync($"An error occurred while adding the employee: {ex.Message}");
                    return errorResponse;
                }
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
                try
                {
                    log.LogInformation($"Updating employee with ID: {id}");

                    var requestBody = await req.ReadAsStringAsync();
                    var updatedEmployeeDto = JsonConvert.DeserializeObject<EmployeeDTO>(requestBody);

                    if (updatedEmployeeDto == null)
                    {
                        log.LogError("Invalid employee data.");
                        var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                        await errorResponse.WriteStringAsync("Invalid employee data provided.");
                        return errorResponse;
                    }

                    var updatedEmployee = await _employeeService.UpdateEmployeeAsync(id, updatedEmployeeDto);

                    var response = req.CreateResponse(updatedEmployee != null ? HttpStatusCode.OK : HttpStatusCode.NotFound);

                    if (updatedEmployee != null)
                    {
                        response.Headers.Add("Content-Type", "application/json");
                        await response.WriteStringAsync(JsonConvert.SerializeObject(updatedEmployee));
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    log.LogError($"Error: {ex.Message}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync($"An error occurred while updating the employee: {ex.Message}");
                    return errorResponse;
                }
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
                try
                {
                    log.LogInformation($"Deleting employee with ID: {id}");

                    var result = await _employeeService.DeleteEmployeeAsync(id);

                    var response = req.CreateResponse(result ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
                    return response;
                }
                catch (Exception ex)
                {
                    log.LogError($"Error: {ex.Message}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync($"An error occurred while deleting the employee: {ex.Message}");
                    return errorResponse;
                }
            }, log, req);
        }
    }
}