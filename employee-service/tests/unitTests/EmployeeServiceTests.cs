// using Interfaces;
// using Models;
// using Moq;
// using Services;
// using Xunit;

// namespace employee_service.Services
// {
//     public class EmployeeServiceTests
//     {
//         private MockRepository mockRepository;

//         private Mock<IEmployeeRepository> mockEmployeeRepository;

//         public EmployeeServiceTests()
//         {
//             this.mockRepository = new MockRepository(MockBehavior.Default);

//             this.mockEmployeeRepository = this.mockRepository.Create<IEmployeeRepository>();
//         }

//         private EmployeeService CreateService()
//         {
//             return new EmployeeService(
//                 this.mockEmployeeRepository.Object);
//         }

//         [Fact]
//         public async Task GetAllEmployeesAsync_DisplayAllEmployees_ShouldReturnAllEmployees()
//         {
//             // Arrange
//             var service = this.CreateService();
//             var employees = new List<Employee>
//             {
//                 new Employee { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Roles = new List<EmployeeRole> { EmployeeRole.Admin, EmployeeRole.HeadTrucker }, Skills = new List<Skill> { Skill.Driving, Skill.Waiter} },
//                 new Employee { EmployeeId = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Roles = new List<EmployeeRole> { EmployeeRole.Staff, EmployeeRole.Chef }, Skills = new List<Skill> { Skill.Cleaning, Skill.Cooking} }
//             };

//             // Setup the mock repository to return the employees
//             this.mockEmployeeRepository
//                 .Setup(repo => repo.GetAllEmployeesAsync())
//                 .ReturnsAsync(employees);

//             try
//             {
//                 // Act
//                 var result = await service.GetAllEmployeesAsync();

//                 // Assert
//                 Assert.NotNull(result);
//                 Assert.Equal(employees, result);

//                 // Log success message only after successful assertions
//                 var resultList = result.ToList();
//                 Console.WriteLine("Success: Employees retrieved.");
//                 Console.WriteLine($"First employee: {resultList[0].FirstName} {resultList[0].LastName}");
//                 Console.WriteLine($"Second employee: {resultList[1].FirstName} {resultList[1].LastName}");

//                 // Verify the mock repository call
//                 this.mockEmployeeRepository.VerifyAll();
//             }
//             catch (Exception ex)
//             {
//                 // Log failure message in case of assertion failure or exception
//                 Console.WriteLine($"Failure: Test failed with exception: {ex.Message}");
//                 throw;  // Rethrow the exception to ensure the test fails properly
//             }
//         }

//         [Fact]
//         public async Task GetAllEmployeesAsync_NoEmployeesFound_ShouldReturnEmptyListWithLog()
//         {
//             // Arrange
//             var service = this.CreateService();

//             // Setup the mock repository to return an empty list
//             this.mockEmployeeRepository
//                 .Setup(repo => repo.GetAllEmployeesAsync())
//                 .ReturnsAsync(new List<Employee>());

//             try
//             {
//                 // Act
//                 var result = await service.GetAllEmployeesAsync();

//                 // Assert
//                 Assert.NotNull(result); 
//                 Assert.Empty(result);   

//                 Console.WriteLine("Success: An empty list was returned.");

//                 this.mockEmployeeRepository.Verify(repo => repo.GetAllEmployeesAsync(), Times.Once);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Failure: Test failed with exception: {ex.Message}");
//                 throw;  
//             }
//         }

//         [Fact]
//         public async Task GetAllEmployeesAsync_WhenRepositoryReturnsNull_ShouldReturnEmptyList()
//         {
//             // Arrange
//             var service = this.CreateService();
            
//             // Setup the mock repository to return null
//             this.mockEmployeeRepository
//                 .Setup(repo => repo.GetAllEmployeesAsync())
//                 .ReturnsAsync(null as List<Employee>);

//             // Act
//             var result = await service.GetAllEmployeesAsync();

//             // Assert
//             Assert.NotNull(result); // Ensure result is not null
//             Assert.Empty(result);    // Ensure it's an empty list
//         }

//         [Fact]
//         public async Task GetAllEmployeesAsync_WithLargeDataSet_ShouldReturnAllEmployees()
//         {
//             // Arrange
//             var service = this.CreateService();
//             var largeEmployeeList = Enumerable.Range(1, 1000)
//                 .Select(i => new Employee { EmployeeId = Guid.NewGuid(), FirstName = $"First{i}", LastName = $"Last{i}" })
//                 .ToList();

//             // Setup the mock repository to return a large list of employees
//             this.mockEmployeeRepository
//                 .Setup(repo => repo.GetAllEmployeesAsync())
//                 .ReturnsAsync(largeEmployeeList);

//             // Act
//             var result = await service.GetAllEmployeesAsync();

//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(1000, result.Count());
//             Assert.Equal("First1", result.First().FirstName); // Check first element
//         }

//         [Fact]
//         public async Task GetAllEmployeesAsync_WithInvalidEmployeeData_ShouldHandleGracefully()
//         {
//             // Arrange
//             var service = this.CreateService();
//             var employees = new List<Employee>
//             {
//                 new Employee { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Roles = new List<EmployeeRole> { EmployeeRole.Admin }, Skills = new List<Skill> { Skill.Driving } },
//                 new Employee { EmployeeId = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith" } // Missing skills and roles
//             };

//             // Setup the mock repository to return employees with missing data
//             this.mockEmployeeRepository
//                 .Setup(repo => repo.GetAllEmployeesAsync())
//                 .ReturnsAsync(employees);

//             // Act
//             var result = await service.GetAllEmployeesAsync();

//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(2, result.Count());  // We expect two employees despite the incomplete data
//             Assert.Contains(result, e => e.FirstName == "Jane" && e.LastName == "Smith");
//         }


//         [Fact]
//         public async Task GetAllEmployeesAsync_VerifyRepositoryCallCount_ShouldBeCalledOnce()
//         {
//             // Arrange
//             var service = this.CreateService();
//             var employees = new List<Employee>
//             {
//                 new Employee { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe" }
//             };

//             // Setup the mock repository to return the employees
//             this.mockEmployeeRepository
//                 .Setup(repo => repo.GetAllEmployeesAsync())
//                 .ReturnsAsync(employees);

//             // Act
//             var result = await service.GetAllEmployeesAsync();

//             // Assert
//             Assert.NotNull(result);
//             Assert.Single(result);  // Expect only one employee in the list

//             // Verify repository was called once
//             this.mockEmployeeRepository.Verify(repo => repo.GetAllEmployeesAsync(), Times.Once);
//         }



// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


//         [Fact]
//         public async Task GetEmployeeByIdAsync_StateUnderTest_ExpectedBehavior()
//         {
//            // Arrange
//            var service = this.CreateService();
//            Guid id = default(global::System.Guid);

//            // Act
//            var result = await service.GetEmployeeByIdAsync(
//                id);

//            // Assert
//            Assert.True(false);
//            this.mockRepository.VerifyAll();
//         }


//         [Fact]
//         public async Task GetEmployeesByRoleAsync_ValidRole_ReturnsEmployees()
//         {
//             // Arrange
//             var role = EmployeeRole.Manager;
//             var employees = new List<Employee>
//             {
//                 new Employee { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Roles = new List<EmployeeRole> { role } },
//                 new Employee { EmployeeId = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Roles = new List<EmployeeRole> { role } }
//             };

//             _mockRepository.Setup(repo => repo.GetEmployeesByRoleAsync(role))
//                 .ReturnsAsync(employees);

//             // Act
//             var result = await _service.GetEmployeesByRoleAsync(role);

//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(2, result.Count());
//             _mockRepository.Verify(repo => repo.GetEmployeesByRoleAsync(role), Times.Once);
//         }

//         [Fact]
//         public async Task AddEmployeeAsync_ValidEmployeeDto_AddsEmployee()
//         {
//             // Arrange
//             var employeeDto = new EmployeeDTO
//             {
//                 FirstName = "John",
//                 LastName = "Doe",
//                 Roles = new List<EmployeeRole> { EmployeeRole.Developer }
//             };

//             var addedEmployee = new Employee
//             {
//                 EmployeeId = Guid.NewGuid(),
//                 FirstName = employeeDto.FirstName,
//                 LastName = employeeDto.LastName,
//                 Roles = employeeDto.Roles
//             };

//             _mockRepository.Setup(repo => repo.AddEmployeeAsync(It.IsAny<Employee>()))
//                 .ReturnsAsync(addedEmployee);

//             // Act
//             var result = await _service.AddEmployeeAsync(employeeDto);

//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(employeeDto.FirstName, result.FirstName);
//             Assert.Equal(employeeDto.LastName, result.LastName);
//             _mockRepository.Verify(repo => repo.AddEmployeeAsync(It.IsAny<Employee>()), Times.Once);
//         }

//         [Fact]
//         public async Task UpdateEmployeeAsync_ValidEmployee_UpdatesEmployee()
//         {
//             // Arrange
//             var employeeId = Guid.NewGuid();
//             var existingEmployee = new Employee
//             {
//                 EmployeeId = employeeId,
//                 FirstName = "John",
//                 LastName = "Doe",
//                 Roles = new List<EmployeeRole> { EmployeeRole.Developer }
//             };

//             var updatedEmployeeDto = new EmployeeDTO
//             {
//                 FirstName = "Jane",
//                 LastName = "Smith",
//                 Roles = new List<EmployeeRole> { EmployeeRole.Manager }
//             };

//             _mockRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId))
//                 .ReturnsAsync(existingEmployee);

//             _mockRepository.Setup(repo => repo.UpdateEmployeeAsync(It.IsAny<Employee>()))
//                 .ReturnsAsync(existingEmployee);

//             // Act
//             var result = await _service.UpdateEmployeeAsync(employeeId, updatedEmployeeDto);

//             // Assert
//             Assert.NotNull(result);
//             Assert.Equal(updatedEmployeeDto.FirstName, result.FirstName);
//             Assert.Equal(updatedEmployeeDto.LastName, result.LastName);
//             _mockRepository.Verify(repo => repo.GetEmployeeByIdAsync(employeeId), Times.Once);
//             _mockRepository.Verify(repo => repo.UpdateEmployeeAsync(It.IsAny<Employee>()), Times.Once);
//         }

//         [Fact]
//         public async Task DeleteEmployeeAsync_ValidId_DeletesEmployee()
//         {
//             // Arrange
//             var employeeId = Guid.NewGuid();

//             _mockRepository.Setup(repo => repo.DeleteEmployeeAsync(employeeId))
//                 .ReturnsAsync(true);

//             // Act
//             var result = await _service.DeleteEmployeeAsync(employeeId);

//             // Assert
//             Assert.True(result);
//             _mockRepository.Verify(repo => repo.DeleteEmployeeAsync(employeeId), Times.Once);
//         }
//     }
// }


using Interfaces;
using Models;
using Moq;
using Services;
using Xunit;

namespace employee_service.Services
{
    public class EmployeeServiceTests
    {
        private readonly MockRepository _mockRepository;
        private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;

        public EmployeeServiceTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Default);
            _mockEmployeeRepository = _mockRepository.Create<IEmployeeRepository>();
        }

        private EmployeeService CreateService()
        {
            return new EmployeeService(_mockEmployeeRepository.Object);
        }

        [Fact]
        public async Task GetAllEmployeesAsync_ShouldReturnAllEmployees()
        {
            try
            {
                // Arrange
                var service = CreateService();
                var employees = new List<Employee>
                {
                    new Employee { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Roles = new List<EmployeeRole> { EmployeeRole.Admin }, Skills = new List<Skill> { Skill.Driving } },
                    new Employee { EmployeeId = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Roles = new List<EmployeeRole> { EmployeeRole.Staff }, Skills = new List<Skill> { Skill.Cooking } }
                };

                _mockEmployeeRepository.Setup(repo => repo.GetAllEmployeesAsync()).ReturnsAsync(employees);

                // Act
                var result = await service.GetAllEmployeesAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Equal(employees, result);
                _mockEmployeeRepository.Verify(repo => repo.GetAllEmployeesAsync(), Times.Once);

                Console.WriteLine("Success: Employees retrieved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task GetAllEmployeesAsync_NoEmployees_ShouldReturnEmptyList()
        {
            try
            {
                // Arrange
                var service = CreateService();
                _mockEmployeeRepository.Setup(repo => repo.GetAllEmployeesAsync()).ReturnsAsync(new List<Employee>());

                // Act
                var result = await service.GetAllEmployeesAsync();

                // Assert
                Assert.NotNull(result);
                Assert.Empty(result);
                _mockEmployeeRepository.Verify(repo => repo.GetAllEmployeesAsync(), Times.Once);

                Console.WriteLine("Success: No employees found, empty list returned.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ValidId_ShouldReturnEmployee()
        {
            try
            {
                // Arrange
                var service = CreateService();
                var employeeId = Guid.NewGuid();
                var employee = new Employee { EmployeeId = employeeId, FirstName = "John", LastName = "Doe" };

                _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(employee);

                // Act
                var result = await service.GetEmployeeByIdAsync(employeeId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(employeeId, result.EmployeeId);
                _mockEmployeeRepository.Verify(repo => repo.GetEmployeeByIdAsync(employeeId), Times.Once);

                Console.WriteLine("Success: Employee retrieved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_InvalidId_ShouldReturnNull()
        {
            try
            {
                // Arrange
                var service = CreateService();
                var employeeId = Guid.NewGuid();

                _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId)).ReturnsAsync((Employee)null);

                // Act
                var result = await service.GetEmployeeByIdAsync(employeeId);

                // Assert
                Assert.Null(result);
                _mockEmployeeRepository.Verify(repo => repo.GetEmployeeByIdAsync(employeeId), Times.Once);

                Console.WriteLine("Success: Employee not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task AddEmployeeAsync_ValidEmployee_ShouldAddEmployee()
        {
            try
            {
                // Arrange
                var service = CreateService();
                var employeeDto = new EmployeeDTO { FirstName = "John", LastName = "Doe", Roles = new List<EmployeeRole> { EmployeeRole.Admin } };
                var employee = new Employee { EmployeeId = Guid.NewGuid(), FirstName = employeeDto.FirstName, LastName = employeeDto.LastName, Roles = employeeDto.Roles };

                _mockEmployeeRepository.Setup(repo => repo.AddEmployeeAsync(It.IsAny<Employee>())).ReturnsAsync(employee);

                // Act
                var result = await service.AddEmployeeAsync(employeeDto);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(employeeDto.FirstName, result.FirstName);
                Assert.Equal(employeeDto.LastName, result.LastName);
                _mockEmployeeRepository.Verify(repo => repo.AddEmployeeAsync(It.IsAny<Employee>()), Times.Once);

                Console.WriteLine("Success: Employee added.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task UpdateEmployeeAsync_ValidEmployee_ShouldUpdateEmployee()
        {
            try
            {
                // Arrange
                var service = CreateService();
                var employeeId = Guid.NewGuid();
                var existingEmployee = new Employee { EmployeeId = employeeId, FirstName = "John", LastName = "Doe" };
                var updatedDto = new EmployeeDTO { FirstName = "Jane", LastName = "Smith", Roles = new List<EmployeeRole> { EmployeeRole.HeadTrucker } };

                _mockEmployeeRepository.Setup(repo => repo.GetEmployeeByIdAsync(employeeId)).ReturnsAsync(existingEmployee);
                _mockEmployeeRepository.Setup(repo => repo.UpdateEmployeeAsync(It.IsAny<Employee>())).ReturnsAsync(existingEmployee);

                // Act
                var result = await service.UpdateEmployeeAsync(employeeId, updatedDto);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(updatedDto.FirstName, result.FirstName);
                Assert.Equal(updatedDto.LastName, result.LastName);
                _mockEmployeeRepository.Verify(repo => repo.GetEmployeeByIdAsync(employeeId), Times.Once);
                _mockEmployeeRepository.Verify(repo => repo.UpdateEmployeeAsync(It.IsAny<Employee>()), Times.Once);

                Console.WriteLine("Success: Employee updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ValidId_ShouldDeleteEmployee()
        {
            try
            {
                // Arrange
                var service = CreateService();
                var employeeId = Guid.NewGuid();

                _mockEmployeeRepository.Setup(repo => repo.DeleteEmployeeAsync(employeeId)).ReturnsAsync(true);

                // Act
                var result = await service.DeleteEmployeeAsync(employeeId);

                // Assert
                Assert.True(result);
                _mockEmployeeRepository.Verify(repo => repo.DeleteEmployeeAsync(employeeId), Times.Once);

                Console.WriteLine("Success: Employee deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failure: {ex.Message}");
                throw;
            }
        }
    }
}
