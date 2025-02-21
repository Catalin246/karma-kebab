using Interfaces;
using Models;

namespace Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;       
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        var employees = await _employeeRepository.GetAllEmployeesAsync();
        if (employees == null || !employees.Any())
        {
            return new List<Employee>(); 
        }

        return employees;
    }

    public async Task<Employee> GetEmployeeByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The provided ID is invalid.", nameof(id));
        }

        var employee = await _employeeRepository.GetEmployeeByIdAsync(id);

        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with ID: {id} not found.");
        }

        return employee;
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role)
    {
        if (!Enum.IsDefined(typeof(EmployeeRole), role))
        {
            throw new ArgumentException("Invalid role specified.", nameof(role));
        }

        var employees = await _employeeRepository.GetEmployeesByRoleAsync(role);
        if (employees == null || !employees.Any())
        {
            throw new InvalidOperationException($"No employees found with the role: {role}.");
        }

        return employees;
    }

    private void ValidateRoles(IEnumerable<EmployeeRole> roles)
    {
        if (roles != null && roles.Any(role => !Enum.IsDefined(typeof(EmployeeRole), role)))
        {
            throw new ArgumentException("One or more roles are invalid.", nameof(roles));
        }
        
    }

   public async Task<Employee> AddEmployeeAsync(EmployeeDTO employeeDto)
{
    if (employeeDto == null)
    {
        throw new ArgumentNullException(nameof(employeeDto), "Employee data cannot be null.");
    }

    ValidateRoles(employeeDto.Roles);

    if (string.IsNullOrEmpty(employeeDto.FirstName) || string.IsNullOrEmpty(employeeDto.LastName) || employeeDto.Roles == null)
    {
        throw new ArgumentException("Required fields are missing in the employee data.");
    }

    Employee employee = new Employee
    {
        EmployeeId = Guid.NewGuid(),
        FirstName = employeeDto.FirstName,
        LastName = employeeDto.LastName,
        Roles = employeeDto.Roles,
        DateOfBirth = null,
        Address = null,
        Payrate = null,
        Skills = new List<Skill>(),
        Email = null
    };

    return await _employeeRepository.AddEmployeeAsync(employee);
}


    public async Task<Employee> UpdateEmployeeAsync(Guid employeeId, EmployeeDTO updatedEmployee)
    {
        if (updatedEmployee == null)
        {
            throw new ArgumentNullException(nameof(updatedEmployee), "Updated employee data cannot be null.");
        }

        if (string.IsNullOrEmpty(updatedEmployee.FirstName) || string.IsNullOrEmpty(updatedEmployee.LastName))
        {
            throw new ArgumentException("First name and last name are required.", nameof(updatedEmployee.FirstName) + " and " + nameof(updatedEmployee.LastName));
        }

        // Check if all roles are valid
        if (updatedEmployee.Roles != null && updatedEmployee.Roles.Any(role => !Enum.IsDefined(typeof(EmployeeRole), role)))
        {
            throw new ArgumentException("One or more roles are invalid.", nameof(updatedEmployee.Roles));
        }

        var existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
        if (existingEmployee == null)
        {
            throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
        }

        if (updatedEmployee.DateOfBirth.HasValue)
        {
            updatedEmployee.DateOfBirth = RemoveTime(updatedEmployee.DateOfBirth.Value);
            updatedEmployee.DateOfBirth = EnsureUtc(updatedEmployee.DateOfBirth.Value);
        }
        else
        {
            updatedEmployee.DateOfBirth = null;
        }

        existingEmployee.Address = updatedEmployee.Address;
        existingEmployee.DateOfBirth = updatedEmployee.DateOfBirth;
        existingEmployee.Email = updatedEmployee.Email;
        existingEmployee.FirstName = updatedEmployee.FirstName;
        existingEmployee.LastName = updatedEmployee.LastName;
        existingEmployee.Payrate = updatedEmployee.Payrate;
        existingEmployee.Roles = updatedEmployee.Roles;
        existingEmployee.Skills = updatedEmployee.Skills;

        return await _employeeRepository.UpdateEmployeeAsync(existingEmployee);
    }


    public async Task<bool> DeleteEmployeeAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The provided ID is invalid.", nameof(id));
        }

        return await _employeeRepository.DeleteEmployeeAsync(id);
    }


    private DateTime RemoveTime(DateTime dateTime)
    {
        return dateTime.Date; // keep only the date part, removing the time.
    }

    private DateTime EnsureUtc(DateTime dateTime)
    {
        // If the DateTime kind is Unspecified or Local, convert it to UTC
        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        if (dateTime.Kind == DateTimeKind.Local)
        {
            return dateTime.ToUniversalTime();
        }

        // If it's already UTC, no change is needed
        return dateTime;
    }

}
