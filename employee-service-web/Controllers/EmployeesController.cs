using Interfaces;
using Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Controllers
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
            _logger.LogInformation("Fetching all employees from Table Storage");
            try
            {
                var employees = await _employeeService.GetAllEmployeesAsync();
                if (employees == null || !employees.Any())
                {
                    _logger.LogInformation("No employees found.");
                    return Ok(new List<EmployeeDTO>()); // Return an empty list
                }

                return Ok(employees); // Return the EmployeeDTO list
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all employees.");
                return StatusCode(500, "Internal server error");
            }
        }

        // Get Employee by ID
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetEmployeeById(Guid id)
        {
            _logger.LogInformation($"Fetching employee with ID: {id} from Table Storage");
            try
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee == null)
                {
                    return NotFound();
                }

                return Ok(employee); // Return the EmployeeDTO directly
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching employee by ID.");
                return StatusCode(500, "Internal server error");
            }
        }

        // Get Employees by Role
        [HttpGet("role/{role:int}")]
        public async Task<IActionResult> GetEmployeeByRole(int role)
        {
            _logger.LogInformation($"Fetching employees with Role: {role} from Table Storage");
            try
            {
                if (!Enum.IsDefined(typeof(EmployeeRole), role))
                {
                    _logger.LogWarning("Invalid role specified.");
                    return BadRequest("Invalid role specified.");
                }

                var employeeRole = (EmployeeRole)role;
                var employees = await _employeeService.GetEmployeesByRoleAsync(employeeRole);
                return Ok(employees); // Return the EmployeeDTO list
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching employees by role.");
                return StatusCode(500, "Internal server error");
            }
        }

        // Add Employee
        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] EmployeeDTO employeeDto)
        {
            _logger.LogInformation("Adding new employee");

            if (employeeDto == null)
            {
                return BadRequest("Invalid employee data provided.");
            }

            try
            {
                // Manually generate a new GUID for the employee
                Guid employeeId = Guid.NewGuid();

                // Add the employee with the generated ID
                var addedEmployee = await _employeeService.AddEmployeeAsync(employeeDto, employeeId);

                // Return CreatedAtAction with the newly created employee ID
                return CreatedAtAction(nameof(GetEmployeeById), new { id = employeeId }, addedEmployee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding the employee.");
                return StatusCode(500, "Internal server error");
            }
        }

        // Update Employee
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] EmployeeDTO updatedEmployeeDto)
        {
            _logger.LogInformation($"Updating employee with ID: {id} in Table Storage");

            if (updatedEmployeeDto == null)
            {
                return BadRequest("Invalid employee data provided.");
            }

            try
            {
                // Step 1: Retrieve the existing employee by ID
                var existingEmployee = await _employeeService.GetEmployeeByIdAsync(id);
                
                // Step 2: Handle case where employee doesn't exist
                if (existingEmployee == null)
                {
                    _logger.LogWarning($"Employee with ID {id} not found.");
                    return NotFound(); // Employee not found
                }

                // Step 3: Update the employee using service method
                var updatedEmployee = await _employeeService.UpdateEmployeeAsync(id, updatedEmployeeDto);

                // Step 4: Handle case where update fails (optional)
                if (updatedEmployee == null)
                {
                    return NotFound(); // Employee couldn't be updated
                }

                // Step 5: Return the updated employee DTO
                return Ok(updatedEmployee); // Return the updated EmployeeDTO
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the employee.");
                return StatusCode(500, "Internal server error");
            }
        }


        // Delete Employee
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            _logger.LogInformation($"Deleting employee with ID: {id} from Table Storage");

            try
            {
                var result = await _employeeService.DeleteEmployeeAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the employee.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
