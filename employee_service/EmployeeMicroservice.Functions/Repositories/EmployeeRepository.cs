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

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var employee = await context.Employees.FindAsync(id);

            return employee;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role)
        {

            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var employees = await context.Employees
                .Where(e => e.Roles.Contains(role))
                .ToListAsync();

            return employees;
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

            context.Employees.Update(employee);
            await context.SaveChangesAsync();

            return employee;
        }

        public async Task<bool> DeleteEmployeeAsync(Guid id)
        {

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var employee = await context.Employees.FindAsync(id);

            context.Employees.Remove(employee);
            await context.SaveChangesAsync();
            return true;
        }
    }
}
