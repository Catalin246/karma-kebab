using Azure.Data.Tables;
using Models;

namespace Database;

public interface ITableStorageService
    {
        Task<TableEntity> GetEntityAsync(string tableName, string partitionKey, string rowKey);
        TableClient GetTableClient(string tableName);
        Task CreateTableIfNotExistsAsync(string tableName);  
    }
