using Microsoft.EntityFrameworkCore;
using Models;

namespace Database;

public class ApplicationDatabase : DbContext
{
    public ApplicationDatabase(DbContextOptions<ApplicationDatabase> options)
        : base(options) { }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

    //{
    //    optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=employeedb;Username=postgres;Password=password;");
    //}

    public DbSet<Employee> Employees { get; set; }
}
