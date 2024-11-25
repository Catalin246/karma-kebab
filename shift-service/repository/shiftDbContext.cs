using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class AzureStorageConfig
{
    public string ConnectionString { get; set; }
}

public class ShiftDbContext : IShiftDbContext
{
    private readonly TableClient _tableClient;
    private const string TableName = "Shifts";

    public ShiftDbContext(IOptions<AzureStorageConfig> options)
    {
        if (options == null) throw new ArgumentNullException(nameof(options));
        
        var connectionString = options.Value.ConnectionString;
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Azure Storage connection string is not configured");
        }

        _tableClient = new TableClient(connectionString, TableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<ShiftEntity> GetShift(string partitionKey, string rowKey)
    {
        try
        {
            var response = await _tableClient.GetEntity<ShiftEntity>(partitionKey, rowKey);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task<IEnumerable<ShiftEntity>> GetShifts(string filter = null)
    {
        var shifts = new List<ShiftEntity>();
        var queryResults = _tableClient.QueryAsync<ShiftEntity>(filter);
        
        await foreach (var shift in queryResults)
        {
            shifts.Add(shift);
        }
        
        return shifts;
    }

    public async Task<ShiftEntity> AddShift(ShiftEntity shift)
    {
        await _tableClient.AddEntity(shift);
        return shift;
    }

    public async Task<ShiftEntity> UpdateShift(ShiftEntity shift)
    {
        await _tableClient.UpdateEntity(shift, shift.ETag);
        return shift;
    }

    public async Task DeleteShift(string partitionKey, string rowKey)
    {
        await _tableClient.DeleteEntity(partitionKey, rowKey);
    }

    public async Task<IEnumerable<ShiftEntity>> GetShiftsByEmployee(Guid employeeId)
    {
        var filter = $"EmployeeId eq '{employeeId}'";
        return await GetShifts(filter);
    }

    public async Task DeleteShiftsByEmployee(Guid employeeId)
    {
        var shifts = await GetShiftsByEmployee(employeeId);
        var deleteTasks = shifts.Select(shift => 
            _tableClient.DeleteEntity(shift.PartitionKey, shift.RowKey));
        
        await Task.WhenAll(deleteTasks);
    }
}