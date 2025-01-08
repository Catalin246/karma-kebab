using Microsoft.EntityFrameworkCore;
using Models;

namespace Database;

public class ApplicationDatabase : DbContext
{
    public ApplicationDatabase(DbContextOptions<ApplicationDatabase> options)
        : base(options)
    { }

    public DbSet<Employee> Employees { get; set; }
}

