using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Database;  
using Interfaces;  
using Repositories; 
using Services; 
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
    .ConfigureFunctionsWorkerDefaults()  // Make sure you call this for worker defaults
    .Build();

// Specify the function app binding to listen on all IP addresses
Environment.SetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT", "Development"); // Optional: Set for local dev
Environment.SetEnvironmentVariable("FUNCTIONS_WORKER_RUNTIME", "dotnet-isolated"); // Required for Azure Functions

host.Run();
