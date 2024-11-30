namespace employee_service.Services;
using Npgsql;
using Database;

public class DatabaseService
{
    private readonly Database _database;

    // Constructor to inject the Database class
    public DatabaseService(Database database)
    {
        _database = database;
    }

    // Method to create the database if it doesn't exist
    public void EnsureDatabaseExists(string databaseName)
    {
        if (!CheckDatabaseExists(databaseName))
        {
            CreateDatabase(databaseName);
        }
    }

    // Check if the database exists
    private bool CheckDatabaseExists(string databaseName)
    {
        var checkDatabaseQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";

        using (var conn = new NpgsqlConnection(_database.ConnectionString))
        {
            try
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(checkDatabaseQuery, conn))
                {
                    var result = cmd.ExecuteScalar();
                    return result != null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }

    // Create the database
    private void CreateDatabase(string databaseName)
    {
        var createDatabaseQuery = $"CREATE DATABASE {databaseName}";

        using (var conn = new NpgsqlConnection(_database.ConnectionString))
        {
            try
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(createDatabaseQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"Database '{databaseName}' created successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    // Method to create tables in the specified database
    public void CreateTables(string databaseName)
    {
        var createTableQuery = @"
            CREATE TABLE employees (
                employee_id UUID PRIMARY KEY,
                date_of_birth DATE NOT NULL,
                first_name VARCHAR(50) NOT NULL,
                last_name VARCHAR(50) NOT NULL,
                address TEXT NOT NULL,
                payrate DECIMAL(10, 2) NOT NULL,
                role INT NOT NULL,
                email VARCHAR(100) NOT NULL,
                skills TEXT[] NULL
            )";

        var connStringWithDb = $"{_database.ConnectionString};Database={databaseName}";
        using (var conn = new NpgsqlConnection(connStringWithDb))
        {
            try
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(createTableQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Employee table created successfully!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}