using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IEmployeeService
    {
        // Fetches all employees from the repository
        Task<IEnumerable<Employee>> GetAllEmployeesAsync();

        // Fetches an employee by EmployeeId
        Task<Employee?> GetEmployeeByIdAsync(Guid employeeId);

        Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role);

        // Adds a new employee to the repository
        Task<EmployeeDTO> AddEmployeeAsync(EmployeeDTO employeeDTO, Guid employeeId);

        // Updates an existing employee in the repository
        Task<EmployeeDTO?> UpdateEmployeeAsync(Guid id, EmployeeDTO updatedEmployeeDTO);

        // Deletes an employee from the repository
        Task<bool> DeleteEmployeeAsync(Guid employeeId);
    }
}
