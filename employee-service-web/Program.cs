using Microsoft.EntityFrameworkCore;
using Database;
using Microsoft.Extensions.DependencyInjection;
using Interfaces;
using Repositories;
using Services;

var builder = WebApplication.CreateBuilder(args);

//// Register DbContext with PostgreSQL connection string from appsettings.json
//builder.Services.AddDbContext<ApplicationDatabase>(options =>
//{
//    var connectionString = builder.Configuration.GetConnectionString("PostgreSQLEntityFramework");
//    if (string.IsNullOrEmpty(connectionString))
//    {
//        throw new InvalidOperationException("Connection string 'PostgreSQLEntityFramework' is not defined.");
//    }
//    options.UseNpgsql(connectionString);
//});

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

app.UseAuthorization();
app.MapControllers();
app.Run();


//using Microsoft.AspNetCore.Builder;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.OpenApi.Models;
//using Middlewares;
//using Services;
//using Interfaces;
//using Repositories;
//using Database;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//// Register DbContext with PostgreSQL connection string
//builder.Services.AddDbContext<ApplicationDatabase>(options =>
//{
//    var connectionString = builder.Configuration.GetConnectionString("PostgreSQLEntityFramework");
//    options.UseNpgsql(connectionString);
//});

//// Register repositories and services
//builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
//builder.Services.AddScoped<IEmployeeService, EmployeeService>();
//builder.Services.AddLogging();
//builder.Services.AddControllers();

//// Swagger/OpenAPI documentation configuration
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "Employee Management API",
//        Version = "v1",
//        Description = "A microservice for managing employee data"
//    });
//});

//// Add RabbitMQ Service (if you're using it for EmployeeService as well, otherwise adjust accordingly)
////builder.Services.AddHttpClient<IRabbitMqService, RabbitMqService>();
////builder.Services.AddHostedService<RabbitMqHostedService>();

//// Build the application
//var app = builder.Build();

//// Enable Swagger UI for development environment
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//// Register the custom GatewayHeaderMiddleware
//app.UseMiddleware<GatewayHeaderMiddleware>();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
