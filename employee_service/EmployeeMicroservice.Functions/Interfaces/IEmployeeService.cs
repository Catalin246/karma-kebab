using employee_service.Models;

namespace employee_service.Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee?> GetEmployeeByIdAsync(Guid id);
    Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role);
    Task<Employee> AddEmployeeAsync(Employee employee);
    Task<Employee?> UpdateEmployeeAsync(Guid id, Employee updatedEmployee);
    Task<bool> DeleteEmployeeAsync(Guid id);
}
