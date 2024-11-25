using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class ShiftService : IShiftService
{
    private readonly IShiftDbContext _context;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(IShiftDbContext context, ILogger<ShiftService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ShiftDto>> GetShifts(DateTime? date, Guid? employeeId, ShiftType? shiftType, Guid? shiftId, Guid? eventId)
    {
        try
        {
            var shifts = await _context.GetShifts();  

            // Apply filters based on parameters
            if (date.HasValue)
            {
                var dateOnly = date.Value.Date;
                shifts = shifts.Where(s => s.StartTime.Date == dateOnly);
            }

            if (employeeId.HasValue)
                shifts = shifts.Where(s => s.EmployeeId == employeeId.Value);

            if (shiftType.HasValue)
                shifts = shifts.Where(s => s.ShiftType == shiftType.Value.ToString());

            if (shiftId.HasValue)
                shifts = shifts.Where(s => s.ShiftId == shiftId.Value);

            return shifts.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shifts");
            throw;
        }
    }

    public async Task<ShiftDto> GetShiftById(Guid shiftId)
    {
        try
        {
            var shiftEntity = await _context.GetShiftById(shiftId);
            return shiftEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift with ID: {ShiftId}", shiftId);
            throw;
        }
    }

    public async Task<ShiftDto> CreateShift(ShiftDto shiftDto)
    {
        try
        {
            var shiftEntity = MapToEntity(shiftDto);
            var createdShift = await _context.AddShift(shiftEntity);
            return MapToDto(createdShift);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift");
            throw;
        }
    }

    public async Task<ShiftDto> UpdateShift(Guid shiftId, ShiftDto shiftDto)
    {
        try
        {
            var shift = await _context.GetShiftById(shiftId);
            if (shift == null) return null;

            // Update the shift entity based on ShiftDto
            shift.StartTime = shiftDto.StartTime;
            shift.EndTime = shiftDto.EndTime;
            shift.ShiftType = shiftDto.ShiftType;
            shift.Status = shiftDto.Status;
            shift.ClockInTime = shiftDto.ClockInTime;
            shift.ClockOutTime = shiftDto.ClockOutTime;

            var updatedShift = await _context.UpdateShift(MapToEntity(shift));
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
        var shiftDto = await _context.GetShiftById(shiftId); 
        if (shiftDto == null) return false;
        var shiftEntity = MapToEntity(shiftDto);
        await _context.DeleteShift(shiftEntity.PartitionKey, shiftEntity.RowKey);
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
            var shifts = await _context.GetShiftsByEmployee(employeeId);
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
            var shifts = await _context.GetShiftsByEmployee(employeeId);
            if (!shifts.Any()) return false;

            await _context.DeleteShiftsByEmployee(employeeId);
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
            var shifts = await _context.GetShifts(null, null, null, null, eventId); 
            if (!shifts.Any()) return null;

            foreach (var shift in shifts)
            {
                shift.StartTime = eventDto.StartTime;
                shift.EndTime = eventDto.EndTime;
                await _context.UpdateShift(shift);  // Update shift in DB
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
