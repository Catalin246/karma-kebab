using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class ShiftEntity : ITableEntity
{
    public string PartitionKey { get; set; }  // employeeId
    public string RowKey { get; set; }        // shiftId
    public DateTimeOffset? Timestamp { get; set; }
    public Azure.ETag ETag { get; set; }

    // Shift properties
    public Guid ShiftId
    {
        get => Guid.Parse(RowKey);
        set => RowKey = value.ToString();
    }
    
    public Guid EmployeeId
    {
        get => Guid.Parse(PartitionKey);
        set => PartitionKey = value.ToString();
    }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string ShiftType { get; set; }  // Will store as string representation
    public string Status { get; set; }     // Will store as string representation
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public decimal ShiftHours { get; set; }

    // Helper methods for enum conversion
    public ShiftType GetShiftTypeEnum()
    {
        return Enum.Parse<ShiftType>(ShiftType);
    }

    public void SetShiftTypeEnum(ShiftType shiftType)
    {
        ShiftType = shiftType.ToString();
    }

    public ShiftStatus GetStatusEnum()
    {
        return Enum.Parse<ShiftStatus>(Status);
    }

    public void SetStatusEnum(ShiftStatus status)
    {
        Status = status.ToString();
    }
}

// Extension methods to help with conversion between Entity and Domain models
public static class ShiftEntityExtensions
{
    public static Shift ToDomainModel(this ShiftEntity entity)
    {
        return new Shift
        {
            ShiftId = entity.ShiftId,
            EmployeeId = entity.EmployeeId,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            ShiftType = entity.GetShiftTypeEnum(),
            Status = entity.GetStatusEnum(),
            ClockInTime = entity.ClockInTime,
            ClockOutTime = entity.ClockOutTime,
            // ShiftHours is computed in the domain model
        };
    }

    public static ShiftEntity ToEntity(this Shift shift)
    {
        var entity = new ShiftEntity
        {
            ShiftId = shift.ShiftId,
            EmployeeId = shift.EmployeeId,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            ClockInTime = shift.ClockInTime,
            ClockOutTime = shift.ClockOutTime,
            ShiftHours = shift.ShiftHours
        };

        entity.SetShiftTypeEnum(shift.ShiftType);
        entity.SetStatusEnum(shift.Status);

        return entity;
    }
}