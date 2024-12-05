// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Azure;

public class ShiftService : IShiftService
{
    private readonly IShiftDbContext _dbContext;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(IShiftDbContext dbContext, ILogger<ShiftService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ShiftDto> CreateShift(CreateShiftDto createshiftDto)
{
    try 
    {
        // Validate input
        if (createshiftDto == null)
            throw new ArgumentNullException(nameof(createshiftDto), "Shift data cannot be null");

        // Parse ShiftType
        if (!Enum.TryParse<ShiftType>(createshiftDto.ShiftType, out var shiftType))
            throw new ArgumentException("Invalid shift type", nameof(createshiftDto.ShiftType));

        // Ensure all DateTime values are converted to UTC
        createshiftDto.StartTime = createshiftDto.StartTime.ToUniversalTime();
        createshiftDto.EndTime = createshiftDto.EndTime.ToUniversalTime();

        var shiftEntity = MapToEntity(createshiftDto);

        // Add the shift to the database
        var savedShift = await _dbContext.AddShift(shiftEntity);

        // Convert back to DTO to return
        return ShiftDbContext.MapToDto(savedShift);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating shift");
        throw; // Rethrow to allow global error handling in the controller
    }
}

    public async Task<ShiftDto> GetShiftById(Guid shiftId)
    {
        return await _dbContext.GetShiftById(shiftId);
    }

    public async Task<IEnumerable<ShiftDto>> GetShifts(DateTime? date = null, Guid? employeeId = null, ShiftType? shiftType = null, Guid? shiftId = null, Guid? eventId = null)
    {
        var shifts = await _dbContext.GetShifts(date, employeeId, shiftType, shiftId, eventId);
        return ShiftDbContext.MapToDtos(shifts);
    }
    // public async Task<ShiftDto> UpdateShift(Guid shiftId, UpdateShiftDto updateShiftDto)
    // {
    //     // Retrieve the existing shift with its ETag
    //     var existingShift = await _dbContext.GetShiftById(shiftId);
    //     if (existingShift == null)
    //         return null;

    //     // Map the DTO to the existing entity
    //     existingShift.StartTime = updateShiftDto.StartTime.ToUniversalTime();
    //     existingShift.EndTime = updateShiftDto.EndTime.ToUniversalTime();
    //     existingShift.ShiftType = updateShiftDto.ShiftType.ToString();
    //     existingShift.Status = updateShiftDto.Status.ToString();
    //     existingShift.ClockInTime = updateShiftDto.ClockInTime?.ToUniversalTime();
    //     existingShift.ClockOutTime = updateShiftDto.ClockOutTime?.ToUniversalTime();

    //     _logger.LogInformation("Updating shift with ID: {ShiftId}", shiftId);

    //     // Pass the ETag when updating
    //     return await _dbContext.UpdateShift(MapToEntity(existingShift));
    // }
public async Task<ShiftDto> UpdateShift(Guid shiftId, UpdateShiftDto updateShiftDto)
{
    try 
    {
        // Validate input
        if (updateShiftDto == null)
            throw new ArgumentNullException(nameof(updateShiftDto), "Update shift data cannot be null");

        // Ensure the PartitionKey and RowKey are correctly set
        string partitionKey = shiftId.ToString();
        string rowKey = shiftId.ToString();

        // Retrieve the existing entity
        var response = await _dbContext.GetShift(partitionKey, rowKey);
        var existingShift = response;

        if (existingShift == null)
            return null;
                    
        await _dbContext.UpdateShift(MapToEntity(updateShiftDto, existingShift));

        return MapToDto(existingShift);
    }
    catch (RequestFailedException ex)
    {
        _logger.LogError(ex, "Azure Storage error updating shift with ID: {ShiftId}. Status: {Status}, ErrorCode: {ErrorCode}", 
            shiftId, ex.Status, ex.ErrorCode);
        throw; // Rethrow to be handled by the controller
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error updating shift with ID: {ShiftId}", shiftId);
        throw;
    }
}
    public async Task<bool> DeleteShift(Guid shiftId)
        {
            try
            {
                var shiftDto = await _dbContext.GetShiftById(shiftId); 
                if (shiftDto == null) return false;
                var shiftEntity = MapToEntity(shiftDto);
                await _dbContext.DeleteShift(shiftEntity.PartitionKey, shiftEntity.RowKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shift with ID: {ShiftId}", shiftId);
                throw;
            }
        }
    public async Task<decimal> GetTotalHoursByEmployee(Guid employeeId)
    {
        try
        {
            var shifts = await _dbContext.GetShiftsByEmployee(employeeId);
            return shifts.Where(s => s.ClockInTime.HasValue && s.ClockOutTime.HasValue)
                         .Sum(s => (decimal)(s.ClockOutTime.Value - s.ClockInTime.Value).TotalHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total hours for employee: {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<bool> DeleteEmployeeAndShifts(Guid employeeId)
    {
        try
        {
            var shifts = await _dbContext.GetShiftsByEmployee(employeeId);
            if (!shifts.Any()) return false;

            await _dbContext.DeleteShiftsByEmployee(employeeId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee and shifts for employee ID: {EmployeeId}", employeeId);
            throw;
        }
    }

    // public async Task<IEnumerable<ShiftDto>> UpdateShiftWithEventChanges(Guid eventId, EventDto eventDto) //THIS ONE NEEDS TO BE DISCUSSED
    // {
    //     try
    //     {
    //         var shifts = await _dbContext.GetShifts(null, null, null, null, eventId); 
    //         if (!shifts.Any()) return null;

    //         foreach (var shift in shifts)
    //         {
    //             shift.StartTime = eventDto.StartTime;
    //             shift.EndTime = eventDto.EndTime;
    //             await _dbContext.UpdateShift(shift);  // Update shift in DB
    //         }

    //         return shifts.Select(MapToDto);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error updating shifts for event: {EventId}", eventId);
    //         throw;
    //     }
    // }
    private static ShiftDto MapToDto(ShiftEntity shift)
{
    return new ShiftDto
    {
        ShiftId = shift.ShiftId,
        StartTime = shift.StartTime,
        EndTime = shift.EndTime,
        EmployeeId = shift.EmployeeId,
        
         // Convert enum to string
        ShiftType = shift.GetShiftTypeEnum(),
        Status = shift.GetStatusEnum(),
        
        ClockInTime = shift.ClockInTime,
        ClockOutTime = shift.ClockOutTime,
        ShiftHours = shift.ShiftHours
    };
}

    private static ShiftEntity MapToEntity(ShiftDto shiftDto) 
    {
        if (shiftDto == null){
            throw new ArgumentNullException(nameof(shiftDto));
        }

        return new ShiftEntity 
        {
            ShiftId = shiftDto.ShiftId,
            StartTime = shiftDto.StartTime,
            EndTime = shiftDto.EndTime,
            EmployeeId = shiftDto.EmployeeId,
            ShiftType = shiftDto.ShiftType.ToString(),
            Status = shiftDto.Status.ToString(),
            ClockInTime = shiftDto.ClockInTime,
            ClockOutTime = shiftDto.ClockOutTime,
            ShiftHours = shiftDto.ShiftHours.HasValue ? shiftDto.ShiftHours.Value : (decimal?)null
        };
    }
    private static ShiftEntity MapToEntity(CreateShiftDto createShiftDto)
    {
        if (createShiftDto == null){
            throw new ArgumentNullException(nameof(createShiftDto));
        }
        var newShiftId = Guid.NewGuid();

        return new ShiftEntity 
        {
            ShiftId = newShiftId,
            EmployeeId = createShiftDto.EmployeeId,
            StartTime = createShiftDto.StartTime,
            EndTime = createShiftDto.EndTime,
            ShiftType = createShiftDto.ShiftType, // Already a string
            Status = ShiftStatus.Unconfirmed.ToString(), // Default status
            ClockInTime = null,
            ClockOutTime = null,
            ShiftHours = null
        };
    }
    private static ShiftEntity MapToEntity(UpdateShiftDto updateShiftDto, ShiftEntity existingEntity)
    {
        if (existingEntity == null)
            throw new ArgumentNullException(nameof(existingEntity), "Existing shift entity must be provided");

        existingEntity.StartTime = updateShiftDto.StartTime;
        existingEntity.EndTime = updateShiftDto.EndTime;
        
        existingEntity.ShiftType = updateShiftDto.ShiftType.ToString();
        if (updateShiftDto.Status != default)
        {
            existingEntity.Status = updateShiftDto.Status.ToString();
        }
        existingEntity.ClockInTime = updateShiftDto.ClockInTime;
        existingEntity.ClockOutTime = updateShiftDto.ClockOutTime;

        return existingEntity;
    }

}

