public class Shift
{
    public Guid ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid EmployeeId { get; set; }
    public ShiftType ShiftType { get; set; }
    public ShiftStatus Status { get; set; }
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public int RoleId { get; set; }  
    public decimal ShiftHours
    {
        get
        {
            if (ClockInTime.HasValue && ClockOutTime.HasValue)
            {
                return (decimal)(ClockOutTime.Value - ClockInTime.Value).TotalHours;
            }
            return (decimal)(EndTime - StartTime).TotalHours;
        }
    }
}

public enum ShiftType
{
    Normal,
    Standby
}

public enum ShiftStatus
{
    Confirmed,
    Unconfirmed,
    Cancelled
}
