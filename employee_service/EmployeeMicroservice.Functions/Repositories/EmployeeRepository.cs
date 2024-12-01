namespace employee_service.Repositories;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Database;
using employee_service.Interfaces;
using employee_service.Models;
using Npgsql;
using Newtonsoft.Json;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly Database _database;

    public EmployeeRepository(Database database)
    {
        _database = database;
    }
    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
    {
        var employees = new List<Employee>();
        var query = "SELECT * FROM employees";

        using (var conn = _database.GetConnection())
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    employees.Add(new Employee
                    {
                        EmployeeId = reader.GetGuid(0),
                        DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(1)),  // Convert DateTime to DateOnly
                        FirstName = reader.GetString(2),
                        LastName = reader.GetString(3),
                        Address = reader.GetString(4),
                        Payrate = reader.GetDecimal(5),
                        Role = (EmployeeRole)reader.GetInt32(6),
                        Email = reader.GetString(7),
                        Skills = reader.IsDBNull(8) ? null : JsonConvert.DeserializeObject<List<Skill>>(JsonConvert.SerializeObject(reader.GetFieldValue<string[]>(8)))
                    });
                }
            }
        }

        return employees;
    }


    public async Task<Employee?> GetEmployeeByIdAsync(Guid id)
    {
        var query = "SELECT * FROM employees WHERE employee_id = @EmployeeId";
        
        using (var conn = _database.GetConnection())
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("EmployeeId", id);
                var reader = await cmd.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new Employee
                    {
                        EmployeeId = reader.GetGuid(0),
                        DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(1)),  // Convert DateTime to DateOnly
                        FirstName = reader.GetString(2),
                        LastName = reader.GetString(3),
                        Address = reader.GetString(4),
                        Payrate = reader.GetDecimal(5),
                        Role = (EmployeeRole)reader.GetInt32(6),
                        Email = reader.GetString(7),
                        Skills = reader.IsDBNull(8) ? null : JsonConvert.DeserializeObject<List<Skill>>(JsonConvert.SerializeObject(reader.GetFieldValue<string[]>(8)))
                    };
                }
                return null;
            }
        }
    }

    public async Task<IEnumerable<Employee>> GetEmployeesByRoleAsync(EmployeeRole role)
    {
        var employees = new List<Employee>();
        var query = "SELECT * FROM employees WHERE role = @Role";

        using (var conn = _database.GetConnection())
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("Role", (int)role);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    employees.Add(new Employee
                    {
                        EmployeeId = reader.GetGuid(0),
                        DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(1)),  // Convert DateTime to DateOnly
                        FirstName = reader.GetString(2),
                        LastName = reader.GetString(3),
                        Address = reader.GetString(4),
                        Payrate = reader.GetDecimal(5),
                        Role = (EmployeeRole)reader.GetInt32(6),
                        Email = reader.GetString(7),
                        Skills = reader.IsDBNull(8) ? null : JsonConvert.DeserializeObject<List<Skill>>(JsonConvert.SerializeObject(reader.GetFieldValue<string[]>(8)))
                    });
                }
            }
        }

        return employees;
    }

    public async Task<Employee> AddEmployeeAsync(Employee employee)
    {
        var query = @"
            INSERT INTO employees (employee_id, date_of_birth, first_name, last_name, address, payrate, role, email, skills)
            VALUES (@EmployeeId, @DateOfBirth, @FirstName, @LastName, @Address, @Payrate, @Role, @Email, @Skills)
            RETURNING *";

        using (var conn = _database.GetConnection())
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("EmployeeId", employee.EmployeeId);
                cmd.Parameters.AddWithValue("DateOfBirth", employee.DateOfBirth.ToDateTime(new TimeOnly(0, 0)));  // Converts DateOnly to DateTime at midnight.
                cmd.Parameters.AddWithValue("FirstName", employee.FirstName);
                cmd.Parameters.AddWithValue("LastName", employee.LastName);
                cmd.Parameters.AddWithValue("Address", employee.Address);
                cmd.Parameters.AddWithValue("Payrate", employee.Payrate);
                cmd.Parameters.AddWithValue("Role", (int)employee.Role);
                cmd.Parameters.AddWithValue("Email", employee.Email);
                cmd.Parameters.AddWithValue("Skills", employee.Skills);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Employee
                    {
                        EmployeeId = reader.GetGuid(0),
                        DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(1)),  // Convert DateTime to DateOnly
                        FirstName = reader.GetString(2),
                        LastName = reader.GetString(3),
                        Address = reader.GetString(4),
                        Payrate = reader.GetDecimal(5),
                        Role = (EmployeeRole)reader.GetInt32(6),
                        Email = reader.GetString(7),
                        Skills = reader.IsDBNull(8) ? null : JsonConvert.DeserializeObject<List<Skill>>(JsonConvert.SerializeObject(reader.GetFieldValue<string[]>(8)))
                    };
                }
                return null;
            }
        }
    }

    public async Task<Employee?> UpdateEmployeeAsync(Guid id, Employee updatedEmployee)
    {
        var query = @"
            UPDATE employees
            SET first_name = @FirstName, last_name = @LastName, date_of_birth = @DateOfBirth, 
                address = @Address, payrate = @Payrate, role = @Role, email = @Email, skills = @Skills
            WHERE employee_id = @EmployeeId
            RETURNING *";

        using (var conn = _database.GetConnection())
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("EmployeeId", id);
                cmd.Parameters.AddWithValue("FirstName", updatedEmployee.FirstName);
                cmd.Parameters.AddWithValue("LastName", updatedEmployee.LastName);
                cmd.Parameters.AddWithValue("DateOfBirth", updatedEmployee.DateOfBirth.ToDateTime(new TimeOnly(0, 0)));  // Convert DateOnly to DateTime at midnight.
                cmd.Parameters.AddWithValue("Address", updatedEmployee.Address);
                cmd.Parameters.AddWithValue("Payrate", updatedEmployee.Payrate);
                cmd.Parameters.AddWithValue("Role", (int)updatedEmployee.Role);
                cmd.Parameters.AddWithValue("Email", updatedEmployee.Email);
                cmd.Parameters.AddWithValue("Skills", updatedEmployee.Skills);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Employee
                    {
                        EmployeeId = reader.GetGuid(0),
                        DateOfBirth = DateOnly.FromDateTime(reader.GetDateTime(1)),  // Convert DateTime to DateOnly
                        FirstName = reader.GetString(2),
                        LastName = reader.GetString(3),
                        Address = reader.GetString(4),
                        Payrate = reader.GetDecimal(5),
                        Role = (EmployeeRole)reader.GetInt32(6),
                        Email = reader.GetString(7),
                        Skills = reader.IsDBNull(8) ? null : JsonConvert.DeserializeObject<List<Skill>>(JsonConvert.SerializeObject(reader.GetFieldValue<string[]>(8)))
                    };
                }
                return null;
            }
        }
    }

    public async Task<bool> DeleteEmployeeAsync(Guid id)
    {
        var query = "DELETE FROM employees WHERE employee_id = @EmployeeId";

        using (var conn = _database.GetConnection())
        {
            await conn.OpenAsync();
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("EmployeeId", id);
                var rowsAffected = await cmd.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
        }
    }

    
}
