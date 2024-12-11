namespace employee_service.Models;

public class EmployeeDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public EmployeeRole Role { get; set; }
    public Skill? Skill { get; set; }
}
