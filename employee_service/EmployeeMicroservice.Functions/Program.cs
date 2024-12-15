using employee_service.Models;  
using employee_service.Database;  
using employee_service.Interfaces;  
using employee_service.Services;  
using employee_service.Repositories;  
using Microsoft.Azure.Functions.Extensions.DependencyInjection;  
using Microsoft.Extensions.Hosting;  
using Microsoft.Extensions.Configuration;  
using Microsoft.EntityFrameworkCore;  
using Microsoft.Extensions.DependencyInjection;  

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, configBuilder) =>  
    {
        // Add local.settings.json explicitly for local development
        configBuilder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)  
                     .AddEnvironmentVariables();  // Environment variables for other deployment environments
    })
    .ConfigureServices((context, services) =>  
    {
        var configuration = context.Configuration;

        // Register DbContextFactory for PostgreSQL with DI
        services.AddDbContextFactory<ApplicationDatabase>(options =>
        {
            // Read the connection string from local.settings.json or environment variables
            var connectionString = configuration["Values:PostgreSQLEntityFramework"];
            options.UseNpgsql(connectionString);
        });

        // Register application services and repositories
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();
    })
    .Build();

host.Run();
