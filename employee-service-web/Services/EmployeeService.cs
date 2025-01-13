using Models;
using Interfaces;
using Microsoft.Extensions.Logging;
using Utility;

namespace Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all employees.");
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            return employees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching employees.");
            throw new ApplicationException("An error occurred while fetching employees.", ex);
        }
    }

    public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Fetching employee with ID {id}.");
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching employee by ID.");
            throw new ApplicationException("An error occurred while fetching employee by ID.", ex);
        }
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role)
    {
        try
        {
            _logger.LogInformation($"Fetching employees with role {role}.");
            var employees = await _employeeRepository.GetEmployeesByRoleAsync(role);
            return employees;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching employees by role.");
            throw new ApplicationException("An error occurred while fetching employees by role.", ex);
        }
    }


public async Task<EmployeeDTO> AddEmployeeAsync(EmployeeDTO employeeDTO, Guid employeeId)
{
    try
    {
        _logger.LogInformation("Adding new employee.");
        
        // Map EmployeeDTO to Employee
        var employee = EmployeeMapper.MapEmployeeDTOToEmployee(employeeDTO);

        // Assign the generated employee ID to the employee
        employee.EmployeeId = employeeId; // Assign the passed GUID to the Employee entity

        // Add the employee to the repository
        var addedEmployee = await _employeeRepository.AddEmployeeAsync(employee);
        
        // Map added Employee back to EmployeeDTO and return    
        return EmployeeMapper.MapEmployeeToEmployeeDTO(addedEmployee);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "An error occurred while adding the employee.");
        throw new ApplicationException("An error occurred while adding the employee.", ex);
    }
}


    public async Task<EmployeeDTO?> UpdateEmployeeAsync(Guid id, EmployeeDTO updatedEmployeeDTO)
    {
        try
        {
            // Step 1: Retrieve the existing employee by ID
            var existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(id);
            
            // Step 2: Handle case where employee doesn't exist
            if (existingEmployee == null)
            {
                _logger.LogWarning($"Employee with ID {id} not found.");
                return null; // or throw an exception if needed
            }

            // Step 3: Map the updated EmployeeDTO to the Employee entity
            var updatedEmployee = EmployeeMapper.MapEmployeeDTOToEmployee(updatedEmployeeDTO);

            // Step 4: Perform the update using the repository method
            var updatedEntity = await _employeeRepository.UpdateEmployeeAsync(updatedEmployee);
            
            // Step 5: Map back the updated Employee entity to EmployeeDTO
            return updatedEntity == null ? null : EmployeeMapper.MapEmployeeToEmployeeDTO(updatedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the employee.");
            throw new ApplicationException("An error occurred while updating the employee.", ex);
        }
    }



    public async Task<bool> DeleteEmployeeAsync(Guid id)
    {
        try
        {
            _logger.LogInformation($"Deleting employee with ID {id}.");
            var result = await _employeeRepository.DeleteEmployeeAsync(id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the employee.");
            return false;
        }
    }

}
