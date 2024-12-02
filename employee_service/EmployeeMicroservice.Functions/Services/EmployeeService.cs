using employee_service.Interfaces;
using employee_service.Models;

namespace employee_service.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        return await _employeeRepository.GetAllEmployeesAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
    {
        return await _employeeRepository.GetEmployeeByIdAsync(id);
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role)
    {
        return await _employeeRepository.GetEmployeesByRoleAsync(role);
    }

    public async Task<Employee> AddEmployeeAsync(EmployeeDTO employeeDto)
    {
        // Map the DTO to the model
        var employee = new Employee
        {
            EmployeeId = Guid.NewGuid(),
            FirstName = employeeDto.FirstName,
            LastName = employeeDto.LastName,
            Role = employeeDto.Role,
            DateOfBirth = null,  
            Address = null,
            Payrate = null,
            Skills = null
        };

        System.Console.WriteLine(employee);
        // Pass this model to the repository layer for saving
        return await _employeeRepository.AddEmployeeAsync(employee);
    }


    public async Task<Employee?> UpdateEmployeeAsync(Guid id, Employee updatedEmployee)
    {
        return await _employeeRepository.UpdateEmployeeAsync(id, updatedEmployee);
    }

    public async Task<bool> DeleteEmployeeAsync(Guid id)
    {
        return await _employeeRepository.DeleteEmployeeAsync(id);
    }
}
