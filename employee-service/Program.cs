using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.EntityFrameworkCore;
using Database;
using Interfaces;
using Middlewares;
using Repositories;
using Services; // Your middleware namespace

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()  // Add Web Application functions middleware configuration
    .ConfigureAppConfiguration((context, configBuilder) =>
    {
        configBuilder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables(); // Add env variables for deployment environments
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // Register database context and application services
        services.AddDbContextFactory<ApplicationDatabase>(options =>
        {
            var connectionString = configuration["Values:PostgreSQLEntityFramework"];
            options.UseNpgsql(connectionString);
        });

        services.AddSingleton<IFunctionsWorkerMiddleware, GatewayHeaderMiddleware>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        
        // Register Middleware
        services.AddSingleton<GatewayHeaderMiddleware>();
    })
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        // Register Custom middleware for the isolated model
        worker.UseMiddleware<GatewayHeaderMiddleware>(); 
    })
    .Build();

Environment.SetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT", "Development");
Environment.SetEnvironmentVariable("FUNCTIONS_WORKER_RUNTIME", "dotnet-isolated");

host.Run();