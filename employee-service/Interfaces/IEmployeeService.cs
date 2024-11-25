using employee_service.Models;

namespace employee_service.Interfaces;

public interface IEmployeeService
{
    Task CreateEmployee(Employee employee);
}
