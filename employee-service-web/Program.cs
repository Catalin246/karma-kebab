using Microsoft.EntityFrameworkCore;
using Database;
using Interfaces;
using Repositories;
using Services;
using Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Explicitly load appsettings.json (ignoring environment-specific configurations)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Determine if the environment is local (Development) or production
var environment = builder.Environment.EnvironmentName;

if (environment == "Development")
{
    // Use connection string from appsettings.json for local development
    var connectionString = builder.Configuration.GetConnectionString("PostgreSQLEntityFramework");
    builder.Services.AddDbContextFactory<ApplicationDatabase>(options =>
    {
        options.UseNpgsql(connectionString);
    });
}
else
{
    // Use connection string from environment variable for production/live environment
    var connectionString = Environment.GetEnvironmentVariable("PostgreSQLEntityFramework");
    builder.Services.AddDbContextFactory<ApplicationDatabase>(options =>
    {
        options.UseNpgsql(connectionString);
    });
}

// Register services, repositories, etc.
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddControllers();

// Swagger and other configurations
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Register the custom GatewayHeaderMiddleware
app.UseMiddleware<GatewayHeaderMiddleware>();

app.UseAuthorization();
app.MapControllers();
app.Run();
