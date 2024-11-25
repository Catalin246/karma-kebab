using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class ShiftDbContext : IShiftDbContext
{
    private readonly TableClient _tableClient;
    private const string TableName = "Shifts";

    public ShiftDbContext(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"] 
            ?? "UseDevelopmentStorage=true"; // Default to Azurite local storage
            
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
