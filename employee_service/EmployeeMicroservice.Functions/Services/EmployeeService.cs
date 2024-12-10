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

    public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role, EmployeeDTO employeeDTO)
    {
        // Map EmployeeDTO to Employee
        var employee = new Employee
        {
            FirstName = employeeDTO.FirstName,
            LastName = employeeDTO.LastName,
            Role = employeeDTO.Role
        };

        // Call repository method with role and employee
        return await _employeeRepository.GetEmployeesByRoleAsync(role, employee);
    }


    public async Task<Employee> AddEmployeeAsync(EmployeeDTO employeeDto)
    {
        // Map EmployeeDTO to Employee
        Employee employee = new Employee
        {
            EmployeeId = Guid.NewGuid(), 
            FirstName = employeeDto.FirstName,
            LastName = employeeDto.LastName,
            Role = employeeDto.Role,
            DateOfBirth = null, 
            Address = null,
            Payrate = null,
            Skills = new List<Skill>(), // Initialize empty list
            Email = null
        };

        // Call repository method with mapped Employee
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
