using System;

public class ClockInDto
{
    public Guid ShiftID {get;set;}
    public DateTime TimeStamp { get; set; }
    public int RoleId {get;set;}
}

public class ShiftCreatedDto
{
    public Guid ShiftId { get; set; }
    public int RoleId {get;set;}
}

public class EventCreatedDto
{
    public Guid EventId { get; set; } // Maps to "eventID"
    public int[] RoleIds { get; set; } // Maps to "roleIDs"
    public DateTime StartTime { get; set; } // Maps to "startTime"
    public DateTime EndTime { get; set; } // Maps to "endTime"
}
