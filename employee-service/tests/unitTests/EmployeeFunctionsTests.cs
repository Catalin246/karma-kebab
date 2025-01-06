//using System;
//using System.Threading.Tasks;
//using Xunit;
//using Moq;
//using Microsoft.Extensions.Logging;
//using Microsoft.Azure.Functions.Worker.Http;
//using Microsoft.Azure.Functions.Worker;
//using Models;
//using Interfaces;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Net;
//using System.IO;
//using System.Text;

//namespace employee_service.tests.unitTests
//{
//    public class EmployeeFunctionsTests
//    {
//        private readonly Mock<IEmployeeService> _mockEmployeeService;
//        private readonly Mock<ILogger<EmployeeFunctions>> _mockLogger;
//        private readonly EmployeeFunctions _employeeFunctions;

//        public EmployeeFunctionsTests()
//        {
//            _mockEmployeeService = new Mock<IEmployeeService>();
//            _mockLogger = new Mock<ILogger<EmployeeFunctions>>(); // Directly mock ILogger<EmployeeFunctions>
//            _employeeFunctions = new EmployeeFunctions(_mockEmployeeService.Object, _mockLogger.Object); // Pass the mocked logger
//        }

//        [Fact]
//        public async Task GetAllEmployees_ShouldReturnAllEmployees_WhenEmployeesExist()
//        {
//            // Arrange
//            var employee1 = new Employee { EmployeeId = Guid.NewGuid(), FirstName = "John", LastName = "Doe", Roles = new List<EmployeeRole> { EmployeeRole.Admin, EmployeeRole.HeadTrucker } };
//            var employee2 = new Employee { EmployeeId = Guid.NewGuid(), FirstName = "Jane", LastName = "Smith", Roles = new List<EmployeeRole> { EmployeeRole.Staff, EmployeeRole.Chef } };

//            var expectedEmployees = new List<Employee> { employee1, employee2 };

//            _mockEmployeeService
//                .Setup(service => service.GetAllEmployeesAsync())
//                .ReturnsAsync(expectedEmployees);

//            // Mock ILogger<EmployeeFunctions>
//            var mockLogger = new Mock<ILogger<EmployeeFunctions>>();

//            // Mock HttpRequestData
//            var req = new Mock<HttpRequestData>(Mock.Of<FunctionContext>());

//            // Mock FunctionContext if needed for more detailed testing
//            var mockFunctionContext = new Mock<FunctionContext>();
            
//            // Create the EmployeeFunctions instance using the mocked logger
//            var employeeFunctions = new EmployeeFunctions(_mockEmployeeService.Object, mockLogger.Object);

//            // Act
//            var result = await employeeFunctions.GetAllEmployees(req.Object, mockFunctionContext.Object);

//            // Assert
//            var createdResult = Assert.IsType<HttpResponseData>(result);
//            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

//            using (var reader = new StreamReader(result.Body, Encoding.UTF8))
//            {
//                var responseContent = await reader.ReadToEndAsync();
//                Assert.Contains("John Doe", responseContent);
//                Assert.Contains("Jane Smith", responseContent);
//            }

//            // Verify logger was called correctly
//            mockLogger.Verify(
//                logger => logger.Log(
//                    It.Is<LogLevel>(level => level == LogLevel.Information),
//                    It.IsAny<EventId>(),
//                    It.IsAny<object>(),
//                    It.IsAny<Exception>(),
//                    It.Is<Func<object, Exception, string>>((state, exception) => state.ToString().Contains("retrieved employees"))
//                ),
//                Times.Once
//            );
//        }

//    }
//}
