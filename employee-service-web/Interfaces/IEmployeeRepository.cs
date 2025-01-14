using Models;

namespace Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<Employee>> GetAllEmployeesAsync();
    Task<Employee> GetEmployeeByIdAsync(Guid id);
    Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole roles);
    Task<Employee> AddEmployeeAsync(Employee employee);
    Task<Employee?> UpdateEmployeeAsync(Employee updatedEmployee);
    Task<bool> DeleteEmployeeAsync(Guid id);
}
