using Microsoft.EntityFrameworkCore;
using Database;
using Interfaces;
using Repositories;
using Services;
using Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext with PostgreSQL connection string from appsettings.json
builder.Services.AddDbContextFactory<ApplicationDatabase>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("PostgreSQLEntityFramework");
    options.UseNpgsql(connectionString);
});

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


