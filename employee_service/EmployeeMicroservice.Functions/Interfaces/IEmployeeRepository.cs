using employee_service.Models;
using employee_service.Database;

namespace employee_service.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();
        Task<Employee?> GetEmployeeByIdAsync(Guid id);
        Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role);
        Task<Employee> AddEmployeeAsync(Employee employee);
        Task<Employee?> UpdateEmployeeAsync(Employee updatedEmployee);
        Task<bool> DeleteEmployeeAsync(Guid id);
    }
}
