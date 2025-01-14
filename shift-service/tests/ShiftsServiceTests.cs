// using System;
// using System.Threading.Tasks;
// using Xunit;
// using Moq;
// using Microsoft.Extensions.Logging;
// //Service layer test : Business logic, data transformations, core functionality
// public class ShiftServiceTests
// {
//     private readonly Mock<IShiftDbContext> _mockDbContext;
//     private readonly Mock<ILogger<ShiftService>> _mockLogger;
//     private readonly ShiftService _shiftService;

//     public ShiftServiceTests()
//     {
//         _mockDbContext = new Mock<IShiftDbContext>();
//         _mockLogger = new Mock<ILogger<ShiftService>>();
//         _shiftService = new ShiftService(_mockDbContext.Object, _mockLogger.Object);
//     }

// [Fact]
// public async Task CreateShift_ValidInput_ReturnsShiftDto()
// {
//     // Arrange
//     var createShiftDto = new CreateShiftDto
//     {
//         StartTime = DateTime.UtcNow,
//         EndTime = DateTime.UtcNow.AddHours(8),
//         EmployeeId = Guid.NewGuid(),
//         ShiftType = ShiftType.Normal.ToString()
//     };

//     var expectedShiftEntity = new ShiftEntity 
//     { 
//         ShiftId = Guid.NewGuid(),
//         StartTime = createShiftDto.StartTime,
//         EndTime = createShiftDto.EndTime,
//         EmployeeId = createShiftDto.EmployeeId,
//         ShiftType = createShiftDto.ShiftType,
//         Status = ShiftStatus.Unconfirmed.ToString(),
//         PartitionKey = createShiftDto.EmployeeId.ToString(), // required property
//         RowKey = Guid.NewGuid().ToString() //  required property
//     };

//     var expectedShiftDto = new ShiftDto { ShiftId = expectedShiftEntity.ShiftId };

//     _mockDbContext
//         .Setup(x => x.AddShift(It.IsAny<ShiftEntity>()))
//         .ReturnsAsync(expectedShiftEntity);

//     // Act
//     var result = await _shiftService.CreateShift(createShiftDto);

//     // Assert
//     Assert.NotNull(result);
//     Assert.Equal(expectedShiftDto.ShiftId, result.ShiftId);
//     _mockDbContext.Verify(x => x.AddShift(It.IsAny<ShiftEntity>()), Times.Once);
// }

//     [Fact]
//     public async Task CreateShift_InvalidShiftType_ThrowsArgumentException()
//     {
//         // Arrange
//         var createShiftDto = new CreateShiftDto
//         {
//             StartTime = DateTime.UtcNow,
//             EndTime = DateTime.UtcNow.AddHours(8),
//             EmployeeId = Guid.NewGuid(),
//             ShiftType = "InvalidShiftType"
//         };

//         // Act & Assert
//         await Assert.ThrowsAsync<ArgumentException>(() => _shiftService.CreateShift(createShiftDto));
//     }

//     [Fact]
//     public async Task DeleteShift_ExistingShift_ReturnsTrue()
//     {
//         // Arrange
//         var shiftId = Guid.NewGuid();
//         var shiftDto = new ShiftDto { ShiftId = shiftId };

//         _mockDbContext
//             .Setup(x => x.GetShiftById(shiftId))
//             .ReturnsAsync(shiftDto);

//         // Act
//         var result = await _shiftService.DeleteShift(shiftId);

//         // Assert
//         Assert.True(result);
//         _mockDbContext.Verify(x => x.DeleteShift(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
//     }

//     [Fact]
//     public async Task DeleteShift_NonExistentShift_ReturnsFalse()
//     {
//         // Arrange
//         var shiftId = Guid.NewGuid();

//         _mockDbContext
//             .Setup(x => x.GetShiftById(shiftId))
//             .ReturnsAsync((ShiftDto)null);

//         // Act
//         var result = await _shiftService.DeleteShift(shiftId);

//         // Assert
//         Assert.False(result);
//         _mockDbContext.Verify(x => x.DeleteShift(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
//     }
// }