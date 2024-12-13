namespace employee_service.Models;

public class EmployeeDTO
{
    public DateTime? DateOfBirth { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public EmployeeRole Role { get; set; }
    public string? Address { get; set; } = default!;
    public decimal? Payrate { get; set; }
    public string? Email { get; set; } = default!;
    public List<Skill>? Skills { get; set; } = new();
}
