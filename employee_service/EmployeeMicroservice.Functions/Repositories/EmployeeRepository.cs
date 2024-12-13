namespace employee_service.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using employee_service.Models;
    using employee_service.Database;
    using employee_service.Interfaces;
    using System.Linq;

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbContextFactory<ApplicationDatabase> _dbContextFactory;

        public EmployeeRepository(IDbContextFactory<ApplicationDatabase> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Employees.ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid employee ID.");
            }

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var employee = await context.Employees.FindAsync(id);

            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {id} not found.");
            }

            return employee;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role)
        {
            // Validate role
            if (!Enum.IsDefined(typeof(EmployeeRole), role))
            {
                throw new ArgumentException("Invalid role specified.", nameof(role));
            }

            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var employees = await context.Employees
                .Where(e => e.Role == role)
                .ToListAsync();

            // Check if no employees are found for the role
            if (employees == null || !employees.Any())
            {
                throw new InvalidOperationException($"No employees found with the role: {role}.");
            }

            return employees;
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee), "Employee cannot be null.");
            }

            await using var context = await _dbContextFactory.CreateDbContextAsync();

            context.Employees.Add(employee);
            await context.SaveChangesAsync();
            return employee;
        }

        public async Task<Employee> UpdateEmployeeAsync(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee), "Employee cannot be null.");
            } 

            await using var context = await _dbContextFactory.CreateDbContextAsync();

            context.Employees.Update(employee);
            await context.SaveChangesAsync();

            return employee;
        }

        public async Task<bool> DeleteEmployeeAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Invalid employee ID.");
            }

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var employee = await context.Employees.FindAsync(id);

            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {id} not found.");
            }

            context.Employees.Remove(employee);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
