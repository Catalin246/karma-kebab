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

// Register Database and DatabaseService
builder.Services.AddSingleton<Database>(serviceProvider =>
{
    var host = "localhost";
    var username = "postgres";
    var password = "password";
    return new Database(host, username, password);
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
