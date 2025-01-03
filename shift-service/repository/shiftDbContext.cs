using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class AzureStorageConfig
{
    public required string ConnectionString { get; set; }
}

public class ShiftDbContext : IShiftDbContext
{
    private readonly TableClient _tableClient;
    private const string TableName = "Shifts";
    public ShiftDbContext(
        IOptions<AzureStorageConfig> options)
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
            return null;
        }
    }

    public async Task<ShiftDto> GetShiftById(Guid shiftId)
    {
        try
        {
            var rowKey = shiftId.ToString();
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

    public async Task<IEnumerable<ShiftEntity>> GetShifts(DateTime? date = null, Guid? employeeId = null, ShiftType? shiftType = null, Guid? shiftId = null, Guid? eventId = null)
    {
        var shifts = new List<ShiftEntity>();
        
        // Start building the filter string
        var filterList = new List<string>();

        if (employeeId.HasValue)
        {
            filterList.Add($"PartitionKey eq '{employeeId}'"); // EmployeeId is stored in PartitionKey
        }

        if (shiftId.HasValue)
        {
            filterList.Add($"RowKey eq '{shiftId}'"); // ShiftId is stored in RowKey
        }

        if (date.HasValue)
        {
            filterList.Add($"StartTime ge '{date.Value:yyyy-MM-dd}' and EndTime le '{date.Value:yyyy-MM-dd}'"); // Assuming date comparison with StartTime and EndTime
        }

        if (shiftType.HasValue)
        {
            filterList.Add($"ShiftType eq '{shiftType}'");
        }

        if (eventId.HasValue)
        {
            // Assuming eventId is a part of the RowKey or another column (adjust if needed)
            filterList.Add($"EventId eq '{eventId}'"); // Adjust depending on how eventId is stored
        }

        // Combine all filters using "and"
        string filter = string.Join(" and ", filterList);

        // If no filters are applied, query all shifts
        var queryResults = string.IsNullOrEmpty(filter) 
            ? _tableClient.QueryAsync<ShiftEntity>() 
            : _tableClient.QueryAsync<ShiftEntity>(filter);
        
        // Collect results
        await foreach (var shift in queryResults)
        {
            shifts.Add(shift);
        }

        return shifts;
    }

    public async Task<ShiftEntity> AddShift(ShiftEntity shift)
{
    try
    {
        if (string.IsNullOrEmpty(shift.PartitionKey))
            throw new ArgumentException("PartitionKey must be set", nameof(shift));
        
        if (string.IsNullOrEmpty(shift.RowKey))
            throw new ArgumentException("RowKey must be set", nameof(shift));

        await _tableClient.AddEntityAsync(shift);
        return shift;
    }
    catch (Azure.RequestFailedException ex)
    {
        throw; 
    }
    catch (Exception ex)
    {
        throw;
    }
}

    public async Task<ShiftEntity> UpdateShift(ShiftEntity shift)
    {
        await _tableClient.UpdateEntityAsync(shift, shift.ETag);
        return shift;
    }

    public async Task DeleteShift(string partitionKey, string rowKey)
    {
        await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

public static ShiftDto MapToDto(ShiftEntity shift)
{
    return new ShiftDto
    {
        ShiftId = shift.ShiftId,
        EmployeeId = shift.EmployeeId,
        ShiftType = Enum.Parse<ShiftType>(shift.ShiftType),
        Status = Enum.Parse<ShiftStatus>(shift.Status),
        StartTime = shift.StartTime, 
        EndTime = shift.EndTime,     
        ClockInTime = shift.ClockInTime, 
        ClockOutTime = shift.ClockOutTime, 
        ShiftHours = shift.ShiftHours.HasValue ? shift.ShiftHours.Value : null
    };
}


    // Convert a collection of ShiftEntities to ShiftDtos
    public static IEnumerable<ShiftDto> MapToDtos(IEnumerable<ShiftEntity> shifts)
    {
        return shifts.Select(MapToDto);
    }
}
