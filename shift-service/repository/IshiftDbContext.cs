using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public interface IShiftDbContext
{
    Task<ShiftEntity> GetShift(string partitionKey, string rowKey);
    Task<IEnumerable<ShiftEntity>> GetShifts(DateTime? date = null, Guid? employeeId = null, ShiftType? shiftType = null, Guid? shiftId = null, Guid? eventId = null);
    Task<ShiftDto> GetShiftById(Guid shiftId);
    Task<ShiftEntity> AddShift(ShiftEntity shift);
    Task<ShiftEntity> UpdateShift(ShiftEntity shift);
    Task DeleteShift(string partitionKey, string rowKey);
    Task<IEnumerable<ShiftEntity>> GetShiftsByEmployee(Guid employeeId);
    Task DeleteShiftsByEmployee(List<ShiftEntity> shifts);
}