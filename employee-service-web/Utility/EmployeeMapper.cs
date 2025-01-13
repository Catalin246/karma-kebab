using Azure.Data.Tables;
using Models;

namespace Utility
{
    public static class EmployeeMapper
    {
        // Map TableEntity to Employee (Entity)
        public static Employee MapTableEntityToEmployee(TableEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "The entity cannot be null.");
            }

            return new Employee
            {
                EmployeeId = Guid.Parse(entity.RowKey), // Assuming RowKey is used for EmployeeId
                FirstName = entity.GetString("FirstName"),
                LastName = entity.GetString("LastName"),
                Address = entity.GetString("Address"),
                Payrate = (decimal?)entity.GetDouble("Payrate"), // Use GetDouble and cast to decimal
                Roles = entity.GetString("Roles")?.Split(",").Select(r => Enum.TryParse<EmployeeRole>(r, out var role) ? role : EmployeeRole.Admin).ToList() ?? new List<EmployeeRole>(),
                Email = entity.GetString("Email"),
                Skills = entity.GetString("Skills")?.Split(",").Select(s => Enum.TryParse<Skill>(s, out var skill) ? skill : Skill.Cleaning).ToList() ?? new List<Skill>(),
                PartitionKey = entity.PartitionKey, // Role as PartitionKey
                RowKey = entity.RowKey // EmployeeId as RowKey
            };
        }

        // Map TableEntity to EmployeeDTO
        public static EmployeeDTO MapTableEntityToEmployeeDTO(TableEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "The entity cannot be null.");
            }

            return new EmployeeDTO
            {
                DateOfBirth = DateTime.TryParse(entity.GetString("DateOfBirth"), out var dateOfBirth) ? dateOfBirth : (DateTime?)null,
                FirstName = entity.GetString("FirstName"),
                LastName = entity.GetString("LastName"),
                Address = entity.GetString("Address"),
                Payrate = (decimal?)entity.GetDouble("Payrate"),
                Roles = entity.GetString("Roles")?.Split(",").Select(r => Enum.TryParse<EmployeeRole>(r, out var role) ? role : EmployeeRole.HeadTrucker).ToList() ?? new List<EmployeeRole>(),
                Email = entity.GetString("Email"),
                Skills = entity.GetString("Skills")?.Split(",").Select(s => Enum.TryParse<Skill>(s, out var skill) ? skill : Skill.Driving).ToList() ?? new List<Skill>()
            };
        }

        // Map Employee (Entity) to TableEntity
        public static TableEntity MapEmployeeToTableEntity(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee), "Employee cannot be null.");
            }

            var entity = new TableEntity(employee.PartitionKey, employee.RowKey) // Using PartitionKey (Role) and RowKey (EmployeeId)
            {
                { "FirstName", employee.FirstName },
                { "LastName", employee.LastName },
                { "Address", employee.Address },
                { "Payrate", employee.Payrate },
                { "Roles", string.Join(",", employee.Roles.Select(role => role.ToString())) },
                { "Email", employee.Email },
                { "Skills", string.Join(",", employee.Skills.Select(skill => skill.ToString())) },
                { "DateOfBirth", employee.DateOfBirth?.ToString() }
            };

            return entity;
        }

        // Map Employee to EmployeeDTO (for sending to client)
        public static EmployeeDTO MapEmployeeToEmployeeDTO(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee), "Employee cannot be null.");
            }

            return new EmployeeDTO
            {
                DateOfBirth = employee.DateOfBirth,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Address = employee.Address,
                Payrate = employee.Payrate,
                Roles = employee.Roles,
                Email = employee.Email,
                Skills = employee.Skills
            };
        }

        // Map EmployeeDTO to Employee (for storing to Table Storage)
        public static Employee MapEmployeeDTOToEmployee(EmployeeDTO employeeDTO)
        {
            if (employeeDTO == null)
            {
                throw new ArgumentNullException(nameof(employeeDTO), "EmployeeDTO cannot be null.");
            }

            return new Employee
            {
                EmployeeId = Guid.NewGuid(), // Generate a new EmployeeId for new employee
                DateOfBirth = employeeDTO.DateOfBirth,
                FirstName = employeeDTO.FirstName,
                LastName = employeeDTO.LastName,
                Address = employeeDTO.Address,
                Payrate = employeeDTO.Payrate,
                Roles = employeeDTO.Roles,
                Email = employeeDTO.Email,
                Skills = employeeDTO.Skills,
                PartitionKey = employeeDTO.Roles?.FirstOrDefault().ToString() ?? "Unknown", // Set PartitionKey to Role (first one)
                RowKey = Guid.NewGuid().ToString() // Generate a new RowKey if creating a new employee
            };
        }
    }
}
