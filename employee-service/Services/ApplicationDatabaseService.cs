using Microsoft.EntityFrameworkCore;
using Database;

namespace Services;

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
