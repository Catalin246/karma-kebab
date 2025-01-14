using Interfaces;
using Models;

namespace Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;


    public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger)
    {
        _employeeRepository = employeeRepository;       
        _logger = logger;
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        // try
        // {
            _logger.LogInformation("Fetching all employees");
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            if (employees == null || !employees.Any())
            {
                return new List<Employee>(); 
            }

            return employees;
        // catch (Exception ex)
        // {
        //     _logger.LogInformation("no employees found.");
        //     throw new ApplicationException("An error occurred while fetching employees.", ex);
        // }
    }

    public async Task<Employee> GetEmployeeByIdAsync(Guid id)
    {
        try 
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
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while fetching employees.", ex);
        }

    }

    public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role)
    {
        try
        {
            // Validate role
            if (!Enum.IsDefined(typeof(EmployeeRole), role))
            {
                throw new ArgumentException("Invalid role specified.", nameof(role));
            }

            // Fetch employees with the specified role
            var employees = await _employeeRepository.GetEmployeesByRoleAsync(role);
            if (employees == null || !employees.Any())
            {
                throw new InvalidOperationException($"No employees found with the role: {role}.");
            }

            return employees;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while fetching employees.", ex);
        }
    }

    public async Task<Employee> AddEmployeeAsync(EmployeeDTO employeeDto)
    {
        // Validate the EmployeeDTO
        if (employeeDto == null)
        {
            throw new ArgumentNullException(nameof(employeeDto), "Employee data cannot be null.");
        }

        if (string.IsNullOrEmpty(employeeDto.FirstName) || string.IsNullOrEmpty(employeeDto.LastName) || employeeDto.Roles == null)
        {
            throw new ArgumentException("Required fields are missing in the employee data.");
        }

        try 
        {
            // Map EmployeeDTO to Employee
            Employee employee = new Employee
            {
                EmployeeId = Guid.NewGuid(), 
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                Roles = employeeDto.Roles,
                DateOfBirth = null, 
                Address = null,
                Payrate = null,
                Skills = new List<Skill>(), // Initialize empty list
                Email = null
            };

            return await _employeeRepository.AddEmployeeAsync(employee);
        }       
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while fetching employees.", ex);
        }     

    }

    public async Task<Employee> UpdateEmployeeAsync(Guid employeeId, EmployeeDTO updatedEmployee)
    {
        try
        {
            // Validate the updatedEmployee object
            if (updatedEmployee == null)
            {
                throw new ArgumentNullException(nameof(updatedEmployee), "Updated employee data cannot be null.");
            }

            // Ensure required fields are not null or empty
            if (string.IsNullOrEmpty(updatedEmployee.FirstName) || string.IsNullOrEmpty(updatedEmployee.LastName))
            {
                throw new ArgumentException("First name and last name are required.", nameof(updatedEmployee.FirstName) + " and " + nameof(updatedEmployee.LastName));
            }

            var existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);

            // Check if the employee exists
            if (existingEmployee == null)
            {
                throw new KeyNotFoundException($"Employee with ID {employeeId} not found.");
            }

            // Ensure DateOfBirth is in UTC and remove time if necessary
            if (updatedEmployee.DateOfBirth.HasValue)
            {
                updatedEmployee.DateOfBirth = RemoveTime(updatedEmployee.DateOfBirth.Value);
                updatedEmployee.DateOfBirth = EnsureUtc(updatedEmployee.DateOfBirth.Value);
            }
            else
            {
                updatedEmployee.DateOfBirth = null;
            }

            // Update the employee entity
            existingEmployee.Address = updatedEmployee.Address;
            existingEmployee.DateOfBirth = updatedEmployee.DateOfBirth;
            existingEmployee.Email = updatedEmployee.Email;
            existingEmployee.FirstName = updatedEmployee.FirstName;
            existingEmployee.LastName = updatedEmployee.LastName;
            existingEmployee.Payrate = updatedEmployee.Payrate;
            existingEmployee.Roles = updatedEmployee.Roles;
            existingEmployee.Skills = updatedEmployee.Skills;

            // Call the repository to update the employee
            return await _employeeRepository.UpdateEmployeeAsync(existingEmployee);
        }
        catch (ArgumentNullException ex)
        {
            // Log and rethrow or handle the exception
            Console.WriteLine($"Validation error: {ex.Message}");
            throw;
        }
        catch (ArgumentException ex)
        {
            // Log and rethrow or handle the exception
            Console.WriteLine($"Validation error: {ex.Message}");
            throw;
        }
        catch (KeyNotFoundException ex)
        {
            // Log and rethrow or handle the exception
            Console.WriteLine($"Not found: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            // Log the unexpected exception and rethrow or return a default error
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            throw;
        }
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


    public async Task<bool> DeleteEmployeeAsync(Guid id)
    {
        // Validate the ID
        if (id == Guid.Empty)
        {
            throw new ArgumentException("The provided ID is invalid.", nameof(id));
        }

        try
        {
            return await _employeeRepository.DeleteEmployeeAsync(id);
        }
        catch (Exception ex)
        {
            // Log and rethrow the exception
            throw new ApplicationException($"Error deleting employee with ID: {id}.", ex);
        }
    }
}
