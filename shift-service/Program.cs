using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

//https://camunda.com/resources/microservices/c/ - really good source for building c# microservices

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.Configure<AzureStorageConfig>(
    builder.Configuration.GetSection("AzureStorage"));

// Register the DbContext
builder.Services.AddScoped<IShiftDbContext, ShiftDbContext>();

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Shifts API", 
        Version = "v1",
        Description = "A microservice for managing employee shifts"
    });
});

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();