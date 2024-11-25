using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public interface IShiftDbContext
{
    Task<ShiftEntity> GetShiftAsync(string partitionKey, string rowKey);
    Task<IEnumerable<ShiftEntity>> GetShiftsAsync(string filter = null);
    Task<ShiftEntity> AddShiftAsync(ShiftEntity shift);
    Task<ShiftEntity> UpdateShiftAsync(ShiftEntity shift);
    Task DeleteShiftAsync(string partitionKey, string rowKey);
    Task<IEnumerable<ShiftEntity>> GetShiftsByEmployeeAsync(Guid employeeId);
    Task DeleteShiftsByEmployeeAsync(Guid employeeId);
}