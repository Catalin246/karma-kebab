using Interfaces;
using Models;
using Moq;
using Services;
using Xunit;

namespace employee_service.Services
{
    public class EmployeeServiceTests
    {
        private MockRepository mockRepository;

        private Mock<IEmployeeRepository> mockEmployeeRepository;

        public EmployeeServiceTests()
        {
            this.mockRepository = new MockRepository(MockBehavior.Default);

            this.mockEmployeeRepository = this.mockRepository.Create<IEmployeeRepository>();
        }

        private EmployeeService CreateService()
        {
            return new EmployeeService(
                this.mockEmployeeRepository.Object);
        }

        [Fact]
        public async Task GetAllEmployeesAsync_DisplayAllEmployees_ShouldReturnAllEmployees()
        {
            // Arrange
            var service = this.CreateService();
            var employees = new List<Employee>
                {
                    new Employee { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Roles = new List<EmployeeRole> { EmployeeRole.Admin, EmployeeRole.HeadTrucker }, Skills = new List<Skill> { Skill.Driving, Skill.Waiter} },
                    new Employee { EmployeeId = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Roles = new List<EmployeeRole> { EmployeeRole.Staff, EmployeeRole.Chef }, Skills = new List<Skill> { Skill.Cleaning, Skill.Cooking} }
                };

            // Setup the mock repository to return the employees
            this.mockEmployeeRepository
                .Setup(repo => repo.GetAllEmployeesAsync())
                .ReturnsAsync(employees);

            // Act
            var result = await service.GetAllEmployeesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(employees, result);

            // Log success details
            var resultList = result.ToList();
            Console.WriteLine($"Success: Employees retrieved.");
            Console.WriteLine($"First employee: {resultList[0].FirstName} {resultList[0].LastName}");
            Console.WriteLine($"Second employee: {resultList[1].FirstName} {resultList[1].LastName}");

            this.mockRepository.VerifyAll();
        }

        //[Fact]
        //public async Task GetEmployeeByIdAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    Guid id = default(global::System.Guid);

        //    // Act
        //    var result = await service.GetEmployeeByIdAsync(
        //        id);

        //    // Assert
        //    Assert.True(false);
        //    this.mockRepository.VerifyAll();
        //}

        //[Fact]
        //public async Task GetEmployeesByRoleAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    EmployeeRole role = default(global::Models.EmployeeRole);

        //    // Act
        //    var result = await service.GetEmployeesByRoleAsync(
        //        role);

        //    // Assert
        //    Assert.True(false);
        //    this.mockRepository.VerifyAll();
        //}

        //[Fact]
        //public async Task AddEmployeeAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    EmployeeDTO employeeDto = null;

        //    // Act
        //    var result = await service.AddEmployeeAsync(
        //        employeeDto);

        //    // Assert
        //    Assert.True(false);
        //    this.mockRepository.VerifyAll();
        //}

        //[Fact]
        //public async Task UpdateEmployeeAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    Guid employeeId = default(global::System.Guid);
        //    EmployeeDTO updatedEmployee = null;

        //    // Act
        //    var result = await service.UpdateEmployeeAsync(
        //        employeeId,
        //        updatedEmployee);

        //    // Assert
        //    Assert.True(false);
        //    this.mockRepository.VerifyAll();
        //}

        //[Fact]
        //public async Task DeleteEmployeeAsync_StateUnderTest_ExpectedBehavior()
        //{
        //    // Arrange
        //    var service = this.CreateService();
        //    Guid id = default(global::System.Guid);

        //    // Act
        //    var result = await service.DeleteEmployeeAsync(
        //        id);

        //    // Assert
        //    Assert.True(false);
        //    this.mockRepository.VerifyAll();
        //}
    }
}
