using Models;

namespace Interfaces;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee> GetEmployeeByIdAsync(Guid id);
    Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole roles);
    Task<Employee> AddEmployeeAsync(EmployeeDTO employeeDto);
    Task<Employee> UpdateEmployeeAsync(Guid id, EmployeeDTO updatedEmployee);
    Task<bool> DeleteEmployeeAsync(Guid id);
}
