namespace employee_service.Database;

public class Database
{
        public string ConnectionString { get; }

        // Constructor to initialize the connection string
        public Database(string host, string username, string password)
        {
            ConnectionString = $"Host={host};Username={username};Password={password}";
        }
}
