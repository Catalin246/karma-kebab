namespace employee_service.Models;

public class Employee
{
    public Guid EmployeeId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Address { get; set; } = default!;
    public decimal? Payrate { get; set; }
    public EmployeeRole Role { get; set; }
    public string? Email { get; set; } = default!;
    // public IEnumerable<Skill> Skills { get; set; } = default!;
    public List<Skill>? Skills { get; set; } = default!;
}
