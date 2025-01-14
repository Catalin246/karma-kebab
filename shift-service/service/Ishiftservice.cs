using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public interface IShiftService
{
    Task<IEnumerable<ShiftDto>> GetShifts(DateTime? startDate, DateTime? endDate, Guid? employeeId, ShiftType? shiftType, Guid? shiftId, Guid? eventId);
    Task<ShiftDto> GetShiftById(Guid shiftId);
    Task<ShiftDto> CreateShift(CreateShiftDto createshiftDto);
    Task<ShiftDto> UpdateShift(Guid shiftId, UpdateShiftDto updateshiftDto);
    Task<bool> DeleteShift(Guid shiftId);
    Task<decimal> GetTotalHoursByEmployee(Guid employeeId);
}