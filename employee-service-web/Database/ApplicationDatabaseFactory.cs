using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Database
{
    public class ApplicationDatabaseFactory : IDesignTimeDbContextFactory<ApplicationDatabase>
    {
        public ApplicationDatabase CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDatabase>();

            // You can use the appsettings.json for your connection string
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Make sure it's correct path
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("PostgreSQLEntityFramework");

            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDatabase(optionsBuilder.Options);
        }
    }
}
