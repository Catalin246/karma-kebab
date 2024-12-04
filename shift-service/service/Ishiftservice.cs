using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public interface IShiftService
{
    Task<IEnumerable<ShiftDto>> GetShifts(DateTime? date, Guid? employeeId, ShiftType? shiftType, Guid? shiftId, Guid? eventId);
    Task<ShiftDto> GetShiftById(Guid shiftId);
    Task<ShiftDto> CreateShift(CreateShiftDto createshiftDto);
    Task<ShiftDto> UpdateShift(Guid shiftId, UpdateShiftDto updateshiftDto);
    Task<bool> DeleteShift(Guid shiftId);
    Task<decimal> GetTotalHoursByEmployee(Guid employeeId);
    Task<bool> DeleteEmployeeAndShifts(Guid employeeId);
    Task<IEnumerable<ShiftDto>> UpdateShiftWithEventChanges(Guid eventId, EventDto eventDto);
}