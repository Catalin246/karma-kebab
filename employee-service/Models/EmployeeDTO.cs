namespace Models;

public class EmployeeDTO
{
    public DateTime? DateOfBirth { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public List<EmployeeRole> Roles { get; set; } = new(); 
    public string? Address { get; set; } = default!;
    public decimal? Payrate { get; set; }
    public string? Email { get; set; } = default!;
    public List<Skill>? Skills { get; set; } = new();
}
