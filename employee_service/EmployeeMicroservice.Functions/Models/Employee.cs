namespace employee_service.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Employee
{
    
    [Key]
    public Guid EmployeeId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Address { get; set; } = default!;
    public decimal? Payrate { get; set; }
    public EmployeeRole Role { get; set; }
    public string? Email { get; set; } = default!;
    
    [Column(TypeName = "text[]")]
    public List<Skill> Skills { get; set; } = new();

}
