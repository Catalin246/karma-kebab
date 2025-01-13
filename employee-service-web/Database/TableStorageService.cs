using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using Models;

namespace Database;

public class TableStorageService : ITableStorageService
{
    private readonly TableServiceClient _tableServiceClient;
    private readonly TableStorageSettings _tableStorageSettings;

    public TableStorageService(IOptions<TableStorageSettings> options)
    {
        _tableStorageSettings = options.Value;
        _tableServiceClient = new TableServiceClient(_tableStorageSettings.ConnectionString);
    }

    public TableClient GetTableClient(string tableName)
    {
        return _tableServiceClient.GetTableClient(tableName);
    }

    public async Task<TableEntity> GetEntityAsync(string tableName, string partitionKey, string rowKey)
    {
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        return await tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
    }

    // Method to ensure the table exists
    public async Task CreateTableIfNotExistsAsync(string tableName)
    {
        var tableClient = _tableServiceClient.GetTableClient(tableName);  // has the connection string

        // Create the table if it doesn't exist
        await tableClient.CreateIfNotExistsAsync();
    }

}
