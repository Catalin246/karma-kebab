using employee_service_web.Controllers;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace employee_service_web.tests.unitTests;

public class EmployeeControllerTests
{
    public class EmployeesControllerTests
    {
        private readonly MockRepository _mockRepository;
        private readonly Mock<IEmployeeService> _mockEmployeeService;
        private readonly Mock<ILogger<EmployeesController>> _mockLogger;

        public EmployeesControllerTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);
            _mockEmployeeService = _mockRepository.Create<IEmployeeService>();
            _mockLogger = _mockRepository.Create<ILogger<EmployeesController>>();
        }

        private EmployeesController CreateController()
        {
            return new EmployeesController(_mockEmployeeService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllEmployees_ReturnsOkResult_WithEmployeeList()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe" },
                new Employee { EmployeeId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe" }
            };

            _mockEmployeeService.Setup(service => service.GetAllEmployeesAsync())
                .ReturnsAsync(employees);

            var controller = CreateController();

            // Act
            var result = await controller.GetAllEmployees();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEmployees = Assert.IsAssignableFrom<IEnumerable<Employee>>(okResult.Value);
            Assert.Equal(2, returnedEmployees.Count());

            // Log the test success
            _mockLogger.Object.LogInformation("Test Passed: GetAllEmployees_ReturnsOkResult_WithEmployeeList");
        }

        [Fact]
        public async Task GetAllEmployees_ReturnsOkResult_WithEmptyList_WhenNoEmployees()
        {
            // Arrange
            var employees = new List<Employee>(); // Empty list of employees
            _mockEmployeeService.Setup(service => service.GetAllEmployeesAsync())
                .ReturnsAsync(employees);

            var controller = CreateController();

            // Act
            var result = await controller.GetAllEmployees();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEmployees = Assert.IsAssignableFrom<IEnumerable<EmployeeDTO>>(okResult.Value);
            Assert.Empty(returnedEmployees); // Ensure the returned list is empty

            // Log the test success
            _mockLogger.Object.LogInformation("Test Passed: GetAllEmployees_ReturnsOkResult_WithEmptyList_WhenNoEmployees");
        }

        [Fact]
        public async Task GetEmployeeById_ReturnsNotFound_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync((Employee)null);

            var controller = CreateController();

            // Act
            var result = await controller.GetEmployeeById(employeeId);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            // Log the test success
            _mockLogger.Object.LogInformation("Test Passed: GetEmployeeById_ReturnsNotFound_WhenEmployeeDoesNotExist");
        }

        [Fact]
        public async Task GetEmployeeById_ReturnsOkResult_WithEmployee()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { EmployeeId = employeeId, FirstName = "John", LastName = "Doe" };
            _mockEmployeeService.Setup(service => service.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(employee);

            var controller = CreateController();

            // Act
            var result = await controller.GetEmployeeById(employeeId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEmployee = Assert.IsType<Employee>(okResult.Value);
            Assert.Equal(employeeId, returnedEmployee.EmployeeId);

            // Log the test success
            _mockLogger.Object.LogInformation("Test Passed: GetEmployeeById_ReturnsOkResult_WithEmployee");
        }

        [Fact]
        public async Task AddEmployee_ReturnsBadRequest_WhenInvalidData()
        {
            // Arrange
            var invalidEmployeeDto = new EmployeeDTO { FirstName = "", LastName = "", Roles = null };

            var controller = CreateController();

            // Act
            var result = await controller.AddEmployee(invalidEmployeeDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid employee data provided.", badRequestResult.Value);

            // Log the test success
            _mockLogger.Object.LogInformation("Test Passed: AddEmployee_ReturnsBadRequest_WhenInvalidData");
        }

        [Fact]
        public async Task AddEmployee_ReturnsCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var employeeDto = new EmployeeDTO
            {
                FirstName = "John",
                LastName = "Doe",
                Roles = new List<EmployeeRole> { EmployeeRole.Admin }
            };
            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid(),
                FirstName = employeeDto.FirstName,
                LastName = employeeDto.LastName,
                Roles = employeeDto.Roles
            };

            _mockEmployeeService.Setup(service => service.AddEmployeeAsync(employeeDto))
                .ReturnsAsync(employee);

            var controller = CreateController();

            // Act
            var result = await controller.AddEmployee(employeeDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GetEmployeeById", createdResult.ActionName);
            Assert.Equal(employee.EmployeeId, createdResult.RouteValues["id"]);

            // Log the test success
            _mockLogger.Object.LogInformation("Test Passed: AddEmployee_ReturnsCreatedAtAction_WhenSuccessful");
        }

        [Fact]
        public async Task UpdateEmployee_ReturnsNotFound_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var employeeId = Guid.NewGuid(); // Non-existent ID
            var updatedEmployeeDto = new EmployeeDTO
            {
                FirstName = "Jane",
                LastName = "Doe",
                Roles = new List<EmployeeRole> { EmployeeRole.Admin }
            };

            _mockEmployeeService.Setup(service => service.UpdateEmployeeAsync(employeeId, updatedEmployeeDto))
                .ThrowsAsync(new KeyNotFoundException("Employee not found"));

            var controller = CreateController();

            // Act
            var result = await controller.UpdateEmployee(employeeId, updatedEmployeeDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result); // Expecting NotFoundObjectResult
            Assert.Equal("Employee not found.", notFoundResult.Value); // Check that the message is correct

            // Log the test success
            _mockLogger.Object.LogInformation("Test Passed: UpdateEmployee_ReturnsNotFound_WhenEmployeeDoesNotExist");
        }

        [Fact]
        public async Task UpdateEmployee_ReturnsBadRequest_WhenRoleIsInvalid()
        {
            // Arrange
            var invalidRole = "InvalidRole";  // Invalid role string that isn't in EmployeeRole enum
            if (!Enum.TryParse(invalidRole, out EmployeeRole employeeRole))
            {
                employeeRole = (EmployeeRole)(-1);  // Assign an invalid value if parsing fails
            }

            var employeeId = Guid.NewGuid();
            var updatedEmployeeDto = new EmployeeDTO
            {
                FirstName = "Jane",
                LastName = "Doe",
                Roles = new List<EmployeeRole> { employeeRole }
            };

            var controller = CreateController();

            // Act
            var result = await controller.UpdateEmployee(employeeId, updatedEmployeeDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid role specified.", badRequestResult.Value);

            // Log the test success
            _mockLogger.Object.LogInformation("Test Passed: UpdateEmployee_ReturnsBadRequest_WhenRoleIsInvalid");
        }

        [Fact]
        public async Task DeleteEmployee_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _mockEmployeeService.Setup(service => service.DeleteEmployeeAsync(employeeId))
                .ReturnsAsync(true);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteEmployee(employeeId);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Log the test success
            _mockLogger.Object.LogInformation("Test Passed: DeleteEmployee_ReturnsNoContent_WhenSuccessful");
        }

        [Fact]
        public async Task DeleteEmployee_ReturnsNotFound_WhenEmployeeDoesNotExist()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _mockEmployeeService.Setup(service => service.DeleteEmployeeAsync(employeeId))
                .ReturnsAsync(false);

            var controller = CreateController();

            // Act
            var result = await controller.DeleteEmployee(employeeId);

            // Assert
            Assert.IsType<NotFoundResult>(result);

            // Log the test success
            _mockLogger.Object.LogInformation("Test Passed: DeleteEmployee_ReturnsNotFound_WhenEmployeeDoesNotExist");
        }
    }
}
