// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class ShiftService : IShiftService
{
    private readonly IShiftDbContext _dbContext;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(IShiftDbContext dbContext, ILogger<ShiftService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ShiftDto> CreateShift(ShiftDto shiftDto)
    {
        try
        {
            // Generate a new ShiftId if not provided
            if (shiftDto.ShiftId == Guid.Empty)
            {
                shiftDto.ShiftId = Guid.NewGuid();
            }

            // Convert ShiftDto to ShiftEntity
            var shiftEntity = new ShiftEntity
            {
                PartitionKey = shiftDto.EmployeeId.ToString(), // Use EmployeeId as PartitionKey
                RowKey = shiftDto.ShiftId.ToString(), // Use ShiftId as RowKey
                EmployeeId = shiftDto.EmployeeId,
                ShiftType = shiftDto.ShiftType,
                Status = shiftDto.Status,
                StartTime = shiftDto.StartTime,
                EndTime = shiftDto.EndTime,
                ClockInTime = shiftDto.ClockInTime,
                ClockOutTime = shiftDto.ClockOutTime,
                ShiftHours = (decimal)shiftDto.ShiftHours
            };

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

    // Implement other methods from IShiftService interface as needed
    public async Task<ShiftDto> GetShiftById(Guid shiftId)
    {
        return await _dbContext.GetShiftById(shiftId);
    }

    public async Task<IEnumerable<ShiftDto>> GetShifts(DateTime? date = null, Guid? employeeId = null, 
        ShiftType? shiftType = null, Guid? shiftId = null, Guid? eventId = null)
    {
        var shifts = await _dbContext.GetShifts(date, employeeId, shiftType, shiftId, eventId);
        return ShiftDbContext.MapToDtos(shifts);
    }
    public async Task<ShiftDto> UpdateShift(Guid shiftId, ShiftDto shiftDto)
    {
        try
        {
            var shift = await _dbContext.GetShiftById(shiftId);
            if (shift == null) return null;

            // Update the shift entity based on ShiftDto
            shift.StartTime = shiftDto.StartTime;
            shift.EndTime = shiftDto.EndTime;
            shift.ShiftType = shiftDto.ShiftType;
            shift.Status = shiftDto.Status;
            shift.ClockInTime = shiftDto.ClockInTime;
            shift.ClockOutTime = shiftDto.ClockOutTime;

            var updatedShift = await _dbContext.UpdateShift(MapToEntity(shift));
            return MapToDto(updatedShift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shift with ID: {ShiftId}", shiftId);
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

    public async Task<IEnumerable<ShiftDto>> UpdateShiftWithEventChanges(Guid eventId, EventDto eventDto)
    {
        try
        {
            var shifts = await _dbContext.GetShifts(null, null, null, null, eventId); 
            if (!shifts.Any()) return null;

            foreach (var shift in shifts)
            {
                shift.StartTime = eventDto.StartTime;
                shift.EndTime = eventDto.EndTime;
                await _dbContext.UpdateShift(shift);  // Update shift in DB
            }

            return shifts.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shifts for event: {EventId}", eventId);
            throw;
        }
    }
    private static ShiftDto MapToDto(ShiftEntity shift)
{
    return new ShiftDto
    {
        ShiftId = shift.ShiftId,
        StartTime = shift.StartTime,
        EndTime = shift.EndTime,
        EmployeeId = shift.EmployeeId,
        
         // Convert enum to string
        ShiftType = shift.GetShiftTypeEnum().ToString(),
        Status = shift.GetStatusEnum().ToString(),
        
        ClockInTime = shift.ClockInTime,
        ClockOutTime = shift.ClockOutTime,
        ShiftHours = (double)shift.ShiftHours
    };
}

private static ShiftEntity MapToEntity(ShiftDto shiftDto)
{
    return new ShiftEntity
    {
        ShiftId = shiftDto.ShiftId,
        StartTime = shiftDto.StartTime,
        EndTime = shiftDto.EndTime,
        EmployeeId = shiftDto.EmployeeId,
        ShiftType = shiftDto.ShiftType,
        Status = shiftDto.Status,    
        ClockInTime = shiftDto.ClockInTime,
        ClockOutTime = shiftDto.ClockOutTime,
        ShiftHours = (decimal)shiftDto.ShiftHours
    };
}

}
