using employee_service.Models;

namespace employee_service.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(Guid id);
    Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role, EmployeeDTO employeeDTO);
    Task<Employee> AddEmployeeAsync(EmployeeDTO employeeDto);
    Task<Employee?> UpdateEmployeeAsync(Guid id, EmployeeDTO updatedEmployee);
    Task<bool> DeleteEmployeeAsync(Guid id);
}
