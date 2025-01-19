using Moq;
using Xunit;
using Services;
using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace employee_service_web.tests.unitTests
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
        private readonly Mock<ILogger<EmployeeService>> _mockLogger;

        public EmployeeServiceTests()
        {
            _mockEmployeeRepository = new Mock<IEmployeeRepository>();
            _mockLogger = new Mock<ILogger<EmployeeService>>();
        }

        private EmployeeService CreateService()
        {
            return new EmployeeService(_mockEmployeeRepository.Object);
        }

        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsEmployees_WhenAvailable()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe" },
                new Employee { EmployeeId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe" }
            };

            _mockEmployeeRepository.Setup(repo => repo.GetAllEmployeesAsync())
                .ReturnsAsync(employees);

            var service = CreateService();

            // Act
            var result = await service.GetAllEmployeesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsEmptyList_WhenNoEmployees()
        {
            // Arrange
            var employees = new List<Employee>(); // Empty list of employees
            _mockEmployeeRepository.Setup(repo => repo.GetAllEmployeesAsync())
                .ReturnsAsync(employees);

            var service = CreateService();

            // Act
            var result = await service.GetAllEmployeesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ReturnsEmployee_WhenExists()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new Employee { EmployeeId = employeeId, FirstName = "John", LastName = "Doe" };

            _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(employee);

            var service = CreateService();

            // Act
            var result = await service.GetEmployeeByIdAsync(employeeId);

            // Assert
            Assert.Equal(employeeId, result.EmployeeId);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ThrowsException_WhenNotFound()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync((Employee)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetEmployeeByIdAsync(employeeId));
        }

        [Fact]
        public async Task GetEmployeesByRoleAsync_ReturnsEmployees_WhenRoleExists()
        {
            // Arrange
            var role = EmployeeRole.Admin;
            var employees = new List<Employee>
            {
                new Employee { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Roles = new List<EmployeeRole> { EmployeeRole.Chef } },
                new Employee { EmployeeId = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe", Roles = new List<EmployeeRole> { EmployeeRole.Chef } }
            };

            _mockEmployeeRepository.Setup(repo => repo.GetEmployeesByRoleAsync(role))
                .ReturnsAsync(employees);

            var service = CreateService();

            // Act
            var result = await service.GetEmployeesByRoleAsync(role);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetEmployeesByRoleAsync_ThrowsException_WhenRoleIsInvalid()
        {
            // Arrange
            var invalidRole = (EmployeeRole)999;
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetEmployeesByRoleAsync(invalidRole));
        }

        [Fact]
        public async Task AddEmployeeAsync_ReturnsEmployee_WhenSuccessful()
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

            _mockEmployeeRepository.Setup(repo => repo.AddEmployeeAsync(It.IsAny<Employee>()))
                .ReturnsAsync(employee);

            var service = CreateService();

            // Act
            var result = await service.AddEmployeeAsync(employeeDto);

            // Assert
            Assert.Equal(employeeDto.FirstName, result.FirstName);
            Assert.Equal(employeeDto.LastName, result.LastName);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_ReturnsUpdatedEmployee_WhenSuccessful()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var updatedEmployeeDto = new EmployeeDTO
            {
                FirstName = "Jane",
                LastName = "Doe",
                Roles = new List<EmployeeRole> { EmployeeRole.Staff }
            };

            var existingEmployee = new Employee
            {
                EmployeeId = employeeId,
                FirstName = "John",
                LastName = "Doe",
                Roles = new List<EmployeeRole> { EmployeeRole.Admin }
            };

            _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync(existingEmployee);

            _mockEmployeeRepository.Setup(repo => repo.UpdateEmployeeAsync(It.IsAny<Employee>()))
                .ReturnsAsync(existingEmployee);

            var service = CreateService();

            // Act
            var result = await service.UpdateEmployeeAsync(employeeId, updatedEmployeeDto);

            // Assert
            Assert.Equal(updatedEmployeeDto.FirstName, result.FirstName);
            Assert.Equal(updatedEmployeeDto.LastName, result.LastName);
            Assert.Equal(updatedEmployeeDto.Roles, result.Roles);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_ThrowsException_WhenEmployeeNotFound()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var updatedEmployeeDto = new EmployeeDTO
            {
                FirstName = "Jane",
                LastName = "Doe",
                Roles = new List<EmployeeRole> { EmployeeRole.Staff }
            };

            _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
                .ReturnsAsync((Employee)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateEmployeeAsync(employeeId, updatedEmployeeDto));
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _mockEmployeeRepository.Setup(repo => repo.DeleteEmployeeAsync(employeeId))
                .ReturnsAsync(true);

            var service = CreateService();

            // Act
            var result = await service.DeleteEmployeeAsync(employeeId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            _mockEmployeeRepository.Setup(repo => repo.DeleteEmployeeAsync(employeeId))
                .ReturnsAsync(false);

            var service = CreateService();

            // Act
            var result = await service.DeleteEmployeeAsync(employeeId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ThrowsException_WhenIdIsInvalid()
        {
            // Arrange
            var invalidId = Guid.Empty;
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.DeleteEmployeeAsync(invalidId));
        }
    }
}
