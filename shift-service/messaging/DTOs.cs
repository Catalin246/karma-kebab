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