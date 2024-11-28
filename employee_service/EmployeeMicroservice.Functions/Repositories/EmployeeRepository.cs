namespace employee_service.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using employee_service.Interfaces;
using employee_service.Models;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly Database _database;
    public EmployeeRepository(Database database)
    {
        _database = database;
        
    }

    public Task<Employee> AddEmployeeAsync(Employee employee)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteEmployeeAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Employee?> GetEmployeeByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role)
    {
        throw new NotImplementedException();
    }

    public Task<Employee?> UpdateEmployeeAsync(Guid id, Employee updatedEmployee)
    {
        throw new NotImplementedException();
    }

}
