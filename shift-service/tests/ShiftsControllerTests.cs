using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
// Controller Layer: HTTP request handling, response formatting, input validation
public class ShiftsControllerTests
{
    private readonly Mock<IShiftService> _mockShiftService;
    private readonly Mock<ILogger<ShiftsController>> _mockLogger;
    private readonly ShiftsController _controller;

    public ShiftsControllerTests()
    {
        _mockShiftService = new Mock<IShiftService>();
        _mockLogger = new Mock<ILogger<ShiftsController>>();
        _controller = new ShiftsController(_mockShiftService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateShift_ValidShift_ReturnsCreatedResult()
    {
        // Arrange
        var createShiftDto = new CreateShiftDto
        {
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(8),
            EmployeeId = Guid.NewGuid(),
            RoleID = 1
        };

        var createdShiftDto = new ShiftDto 
        { 
            ShiftId = Guid.NewGuid(),
            StartTime = createShiftDto.StartTime,
            EndTime = createShiftDto.EndTime
        };

        _mockShiftService
            .Setup(x => x.CreateShift(It.IsAny<CreateShiftDto>()))
            .ReturnsAsync(createdShiftDto);

        // Act
        var result = await _controller.CreateShift(createShiftDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var apiResponse = Assert.IsType<ApiResponse>(createdResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("Shift created successfully", apiResponse.Message);
    }

    [Fact]
    public async Task DeleteShift_ExistingShift_ReturnsNoContent()
    {
        // Arrange
        var shiftId = Guid.NewGuid();
        _mockShiftService
            .Setup(x => x.DeleteShift(shiftId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteShift(shiftId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteShift_NonExistentShift_ReturnsNotFound()
    {
        // Arrange
        var shiftId = Guid.NewGuid();
        _mockShiftService
            .Setup(x => x.DeleteShift(shiftId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteShift(shiftId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}