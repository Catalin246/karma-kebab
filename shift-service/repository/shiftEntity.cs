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
    public required string ShiftType { get; set; }  // Will store as string representation
    public required string Status { get; set; }     // Will store as string representation
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public decimal? ShiftHours { get; set; }

    // Helper methods for enum conversion
    public ShiftType GetShiftTypeEnum()
    {
        return Enum.Parse<ShiftType>(ShiftType, ignoreCase: true);
    }

    public void SetShiftTypeEnum(ShiftType shiftType) //not used
    {
        ShiftType = shiftType.ToString();
    }

    public ShiftStatus GetStatusEnum()
    {
        return Enum.Parse<ShiftStatus>(Status, ignoreCase: true);
    }

    public void SetStatusEnum(ShiftStatus status) //not used
    {
        Status = status.ToString();
    }
}