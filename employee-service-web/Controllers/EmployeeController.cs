using Interfaces;
using Models;
using Microsoft.AspNetCore.Mvc;

namespace employee_service_web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeesController> _logger;

        // Constructor injection for EmployeeService
        public EmployeesController(IEmployeeService employeeService, ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

       // Get All Employees
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            return await ExceptionService.HandleRequestAsync(async () =>
            {
                _logger.LogInformation("Fetching all employees");
                var employees = await _employeeService.GetAllEmployeesAsync();

                if (employees == null || !employees.Any())
                {
                    _logger.LogInformation("No employees found.");
                    return Ok(new List<EmployeeDTO>()); // Return an empty list if no employees are found
                }

                return Ok(employees);
            }, _logger, Request, Response);
        }

        // Get Employee by ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            return await ExceptionService.HandleRequestAsync(async () =>
            {
                _logger.LogInformation($"Fetching employee with ID: {id}");
                var employee = await _employeeService.GetEmployeeByIdAsync(id);

                if (employee == null)
                {
                    return NotFound();
                }

                return Ok(employee);
            }, _logger, Request, Response);
        }

        // Get By Role
        [HttpGet("role/{role:int}")]
        public async Task<IActionResult> GetEmployeeByRole(int role)
        {
            return await ExceptionService.HandleRequestAsync(async () =>
            {
                _logger.LogInformation($"Fetching employees with Role: {role}");

                if (!Enum.IsDefined(typeof(EmployeeRole), role))
                {
                    _logger.LogWarning("Invalid role specified.");
                    return BadRequest("Invalid role specified.");
                }

                var employeeRole = (EmployeeRole)role;
                var employees = await _employeeService.GetEmployeesByRoleAsync(employeeRole);

                return Ok(employees);
            }, _logger, Request, Response);
        }

        // Add Employee
        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] EmployeeDTO employeeDto)
        {
            return await ExceptionService.HandleRequestAsync(async () =>
            {
                _logger.LogInformation("Adding new employee");

                if (employeeDto == null || string.IsNullOrEmpty(employeeDto.FirstName) || string.IsNullOrEmpty(employeeDto.LastName) || employeeDto.Roles == null)
                {
                    return BadRequest("Invalid employee data provided.");
                }

                var addedEmployee = await _employeeService.AddEmployeeAsync(employeeDto);

                return CreatedAtAction(nameof(GetEmployeeById), new { id = addedEmployee.EmployeeId }, addedEmployee);
            }, _logger, Request, Response);
        }

        // Update Employee
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] EmployeeDTO updatedEmployeeDto)
        {
            return await ExceptionService.HandleRequestAsync(async () =>
            {
                _logger.LogInformation($"Updating employee with ID: {id}");

                // Check if the updated employee data is null
                if (updatedEmployeeDto == null)
                {
                    return BadRequest("Invalid employee data provided.");
                }

                // Validate roles in the updated employee DTO
                foreach (var role in updatedEmployeeDto.Roles)
                {
                    if (!Enum.IsDefined(typeof(EmployeeRole), role))
                    {
                        return BadRequest("Invalid role specified.");
                    }
                }

                // Check if the employee ID exists
                var employeeExists = await _employeeService.GetEmployeeByIdAsync(id);
                if (employeeExists == null)  // Change this to null check instead of ID mismatch
                {
                    return NotFound("Employee not found.");
                }

                // Proceed with updating the employee
                var updatedEmployee = await _employeeService.UpdateEmployeeAsync(id, updatedEmployeeDto);

                return Ok(updatedEmployee);
            }, _logger, Request, Response);
        }


        // Delete Employee
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            return await ExceptionService.HandleRequestAsync(async () =>
            {
                _logger.LogInformation($"Deleting employee with ID: {id}");

                var result = await _employeeService.DeleteEmployeeAsync(id);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }, _logger, Request, Response);
        }
    }
}