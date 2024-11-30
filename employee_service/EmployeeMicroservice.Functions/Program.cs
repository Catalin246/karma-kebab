using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using employee_service.Database;
using employee_service.Services;
using Microsoft.Extensions.DependencyInjection;
using employee_service.Interfaces; 
using employee_service.Repositories; 

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Get the connection string from the environment variables (local.settings.json)
var connectionString = Environment.GetEnvironmentVariable("ConnectionString");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("The PostgreSQL connection string is missing.");
}

// Register Database and DatabaseService
builder.Services.AddSingleton<Database>(serviceProvider =>
{
    return new Database(connectionString);  // Pass the connection string to the Database constructor
});
builder.Services.AddSingleton<DatabaseService>();

// Register repositories and services
builder.Services.AddSingleton<IEmployeeRepository, EmployeeRepository>(); 
builder.Services.AddSingleton<IEmployeeService, EmployeeService>(); 

// Ensure the database and tables are created on startup
var databaseService = builder.Services.BuildServiceProvider().GetRequiredService<DatabaseService>();
databaseService.EnsureDatabaseExists("employeedb");
databaseService.CreateTables("employeedb");

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
