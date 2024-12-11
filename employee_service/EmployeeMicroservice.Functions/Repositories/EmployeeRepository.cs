namespace employee_service.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using employee_service.Models;
    using employee_service.Database;
    using employee_service.Interfaces;

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbContextFactory<ApplicationDatabase> _dbContextFactory;

        public EmployeeRepository(IDbContextFactory<ApplicationDatabase> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            // Create a new DbContext instance for each method call
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Employees.ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Employees.FindAsync(id);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role, Employee employee)
        {
            // Ensure the employee exists in the database
            var existingEmployee = await GetEmployeeByIdAsync(employee.EmployeeId);
            if (existingEmployee == null)
            {
                throw new ArgumentException("The specified employee does not exist.");
            }

            // Create a new DbContext instance for the query
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Employees
                .Where(e => e.Role == role)
                .ToListAsync();
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.Employees.Add(employee);
            await context.SaveChangesAsync();
            return employee;
        }

        public async Task<Employee> UpdateEmployeeAsync(Employee employee)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            // Attach and update the entity
            context.Employees.Update(employee);
            await context.SaveChangesAsync();

            return employee;
        }


        public async Task<bool> DeleteEmployeeAsync(Guid id)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var employee = await context.Employees.FindAsync(id);
            if (employee == null) return false;

            context.Employees.Remove(employee);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
