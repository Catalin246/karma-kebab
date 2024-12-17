namespace employee_service.Services;

using Microsoft.EntityFrameworkCore;
using employee_service.Database;

public class ApplicationDatabaseService
{
    private readonly ApplicationDatabase _context;

    public ApplicationDatabaseService(ApplicationDatabase context)
    {
        _context = context;
    }

    public void EnsureDatabaseExists()
    {
        _context.Database.Migrate();  // Ensures the database is created and migrations are applied
    }
}
