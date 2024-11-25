using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public class ShiftService : IShiftService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(ApplicationDbContext context, ILogger<ShiftService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ShiftDto>> GetShifts(DateTime? date, Guid? employeeId, ShiftType? shiftType, Guid? shiftId)
    {
        try
        {
            var query = _context.Shifts.AsQueryable();

            if (date.HasValue)
            {
                var dateOnly = date.Value.Date;
                query = query.Where(s => s.StartTime.Date == dateOnly);
            }

            if (employeeId.HasValue)
                query = query.Where(s => s.EmployeeId == employeeId.Value);

            if (shiftType.HasValue)
                query = query.Where(s => s.ShiftType == shiftType.Value);

            if (shiftId.HasValue)
                query = query.Where(s => s.ShiftId == shiftId.Value);

            var shifts = await query.ToListAsync();
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
            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.ShiftId == shiftId);

            return shift != null ? MapToDto(shift) : null;
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
            var shift = new Shift
            {
                ShiftId = Guid.NewGuid(),
                StartTime = shiftDto.StartTime,
                EndTime = shiftDto.EndTime,
                EmployeeId = shiftDto.EmployeeId,
                ShiftType = shiftDto.ShiftType,
                Status = ShiftStatus.Unconfirmed // Default status for new shifts is unconfirmed
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return MapToDto(shift);
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
            var shift = await _context.Shifts.FindAsync(shiftId);
            if (shift == null) return null;

            // Update basic properties
            shift.StartTime = shiftDto.StartTime;
            shift.EndTime = shiftDto.EndTime;
            shift.ShiftType = shiftDto.ShiftType;
            shift.Status = shiftDto.Status;

            // Handle clock in/out times if provided
            if (shiftDto.ClockInTime.HasValue)
                shift.ClockInTime = shiftDto.ClockInTime;
            
            if (shiftDto.ClockOutTime.HasValue)
                shift.ClockOutTime = shiftDto.ClockOutTime;

            await _context.SaveChangesAsync();
            return MapToDto(shift);
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
            var shift = await _context.Shifts.FindAsync(shiftId);
            if (shift == null) return false;

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();
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
            var shifts = await _context.Shifts
                .Where(s => s.EmployeeId == employeeId && s.ClockOutTime.HasValue && s.ClockInTime.HasValue)
                .ToListAsync();

            return shifts.Sum(s => (decimal)(s.ClockOutTime.Value - s.ClockInTime.Value).TotalHours);
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
            var shifts = await _context.Shifts
                .Where(s => s.EmployeeId == employeeId)
                .ToListAsync();

            if (!shifts.Any()) return false;

            _context.Shifts.RemoveRange(shifts);
            await _context.SaveChangesAsync();
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
            var shifts = await _context.Shifts
                .Where(s => s.EventId == eventId)
                .ToListAsync();

            if (!shifts.Any()) return null;

            foreach (var shift in shifts)
            {
                shift.StartTime = eventDto.StartTime;
                shift.EndTime = eventDto.EndTime;
            }

            await _context.SaveChangesAsync();
            return shifts.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shifts for event: {EventId}", eventId);
            throw;
        }
    }

    private static ShiftDto MapToDto(Shift shift)
    {
        return new ShiftDto
        {
            ShiftId = shift.ShiftId,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            EmployeeId = shift.EmployeeId,
            ShiftType = shift.ShiftType,
            Status = shift.Status,
            ClockInTime = shift.ClockInTime,
            ClockOutTime = shift.ClockOutTime,
            ShiftHours = shift.ShiftHours
        };
    }
}