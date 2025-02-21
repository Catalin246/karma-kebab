﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Models;

public class Employee
{

    [Key]
    public Guid EmployeeId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Address { get; set; } = default!;
    public decimal? Payrate { get; set; }
    public List<EmployeeRole> Roles { get; set; }
    public string? Email { get; set; } = default!;

    [Column(TypeName = "text[]")]
    public List<Skill> Skills { get; set; } = new();

}
