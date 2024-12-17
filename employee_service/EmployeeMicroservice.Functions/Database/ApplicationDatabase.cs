namespace employee_service.Database;

using Microsoft.EntityFrameworkCore;
using employee_service.Models;

public class ApplicationDatabase : DbContext
{
    public ApplicationDatabase(DbContextOptions<ApplicationDatabase> options)
        : base(options){}

    public DbSet<Employee> Employees { get; set; }
}
