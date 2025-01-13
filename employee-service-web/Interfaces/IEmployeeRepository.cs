using Models;


namespace Interfaces
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();  // Returns Employee entities
        Task<Employee> GetEmployeeByIdAsync(Guid id);       // Returns a single Employee entity
        Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role);  // Returns Employee entities by role
        Task<Employee> AddEmployeeAsync(Employee employee);  // Accepts and returns Employee
        Task<Employee?> UpdateEmployeeAsync(Employee updatedEmployee);  // Accepts and returns Employee
        Task<bool> DeleteEmployeeAsync(Guid id);  // Returns true if deleted, false if not
    }
}
