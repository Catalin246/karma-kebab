


var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Database and DatabaseService
builder.Services.AddSingleton<Database>(serviceProvider =>
{
    var host = "localhost";
    var username = "postgres";
    var password = "password";
    return new Database(host, username, password);
});

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<EmployeeService>();


var app = builder.Build();

// Ensure the database and tables are created on startup
var databaseService = app.Services.GetRequiredService<DatabaseService>();
databaseService.EnsureDatabaseExists("employeedb");
databaseService.CreateTables("employeedb");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
