using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var response = await _tableClient.GetEntityAsync<ShiftEntity>(partitionKey, rowKey);
            return response.Value;
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            // Return null if the entity is not found
            return null;
        }
    }

    public async Task<ShiftDto> GetShiftById(Guid shiftId)
    {
        try
        {
            var rowKey = shiftId.ToString(); // RowKey is the shiftId in string format
            var queryResults = _tableClient.QueryAsync<ShiftEntity>(e => e.RowKey == rowKey);
            await foreach (var shiftEntity in queryResults)
            {
                return MapToDto(shiftEntity);
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving the shift.", ex);
        }
    }

    // Get all shifts for a specific employee
    public async Task<IEnumerable<ShiftEntity>> GetShiftsByEmployee(Guid employeeId)
    {
        var filter = $"EmployeeId eq '{employeeId}'";
        var shifts = new List<ShiftEntity>();
        
        var queryResults = _tableClient.QueryAsync<ShiftEntity>(filter);
        await foreach (var shift in queryResults)
        {
            shifts.Add(shift);
        }
        
        return shifts;
    }

    public async Task<IEnumerable<ShiftEntity>> GetShifts(string? filter = null)
    {
        var shifts = new List<ShiftEntity>();
        
        var queryResults = string.IsNullOrEmpty(filter) 
            ? _tableClient.QueryAsync<ShiftEntity>() 
            : _tableClient.QueryAsync<ShiftEntity>(filter);
        
        await foreach (var shift in queryResults)
        {
            shifts.Add(shift);
        }

        return shifts;
    }

    // Add a new shift
    public async Task<ShiftEntity> AddShift(ShiftEntity shift)
    {
        await _tableClient.AddEntityAsync(shift);
        // Return the same shift object as it was successfully added
        return shift;
    }

    // Update an existing shift
    public async Task<ShiftEntity> UpdateShift(ShiftEntity shift)
    {
        await _tableClient.UpdateEntityAsync(shift, shift.ETag);
        return shift;
    }

    // Delete a shift by partition key and row key
    public async Task DeleteShift(string partitionKey, string rowKey)
    {
        await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        // No return value as it's a delete operation
    }

    public async Task DeleteShiftsByEmployee(Guid employeeId)
    {
        var shifts = await GetShiftsByEmployee(employeeId);

        var deleteTasks = shifts.Select(shift => 
            _tableClient.DeleteEntityAsync(shift.PartitionKey, shift.RowKey));  // Use DeleteEntityAsync
        
        // Await all delete operations concurrently
        await Task.WhenAll(deleteTasks);
    }

    // Convert ShiftEntity to ShiftDto
    public static ShiftDto MapToDto(ShiftEntity shift)
    {
        return new ShiftDto
        {
            ShiftId = Guid.TryParse(shift.RowKey, out var shiftId) ? shiftId : Guid.Empty, 
            ShiftType = shift.ShiftType.ToString(), // Convert Enum to string
            Status = shift.Status.ToString(), // Convert Enum to string
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            EmployeeId = shift.EmployeeId,
            ClockInTime = shift.ClockInTime,
            ClockOutTime = shift.ClockOutTime,
            ShiftHours = (double)shift.ShiftHours
        };
    }

    // Convert a collection of ShiftEntities to ShiftDtos
    public static IEnumerable<ShiftDto> MapToDtos(IEnumerable<ShiftEntity> shifts)
    {
        return shifts.Select(MapToDto);
    }
}
