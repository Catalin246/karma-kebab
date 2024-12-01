// DTO for Shift
public class ShiftDto
{
    public Guid ShiftId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid EmployeeId { get; set; }
    public string ShiftType { get; set; }
    public string Status { get; set; }
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public double ShiftHours { get; set; }
}

// DTO for creating a shift
public class CreateShiftDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid EmployeeId { get; set; }
    public ShiftType ShiftType { get; set; }
}

// DTO for updating a shift
public class UpdateShiftDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public ShiftType ShiftType { get; set; }
    public ShiftStatus Status { get; set; }
}

// DTO for clocking in or clocking out
public class ClockInOutDto
{
    public DateTime TimeStamp { get; set; }
}
