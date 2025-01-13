using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Database;
using Interfaces;
using Repositories;
using Services;
using Controllers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Bind TableStorageSettings to the "TableStorage" section in appsettings.json
builder.Services
    .AddOptions<TableStorageSettings>()
    .Bind(builder.Configuration.GetSection("TableStorage"))
    .ValidateDataAnnotations()
    .Validate(settings => !string.IsNullOrEmpty(settings.ConnectionString),
              "Connection string must be provided");

// Register TableServiceClient for Azure Table Storage
builder.Services.AddSingleton<TableServiceClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<TableStorageSettings>>().Value;
    return new TableServiceClient(settings.ConnectionString);
});

// Register services, repositories, and other dependencies
builder.Services.AddSingleton<ITableStorageService, TableStorageService>();
builder.Services.AddSingleton<IEmployeeRepository, EmployeeRepository>(); // Adjust as needed
builder.Services.AddSingleton<IEmployeeService, EmployeeService>(); // Adjust as needed

builder.Services.AddHttpClient<EmployeesController>(); // Adjust as needed

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
