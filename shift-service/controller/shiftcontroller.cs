using Microsoft.AspNetCore.Mvc;
using Azure;
using System.Text.Json;
using Services;
// using Models;

[ApiController]
[Route("[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly ILogger<ShiftsController> _logger;
    private readonly IRabbitMqService _rabbitMqService;

    public ShiftsController(IShiftService shiftService, ILogger<ShiftsController> logger)
    {
        _shiftService = shiftService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse>> ListShifts(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] ShiftType? shiftType = null,
        [FromQuery] Guid? shiftId = null, 
        [FromQuery] Guid? eventId = null)
    {
        try
        {
            var shifts = await _shiftService.GetShifts(startDate, endDate, employeeId, shiftType, shiftId, eventId);
            return Ok(new ApiResponse 
            { 
                Success = true,
                Message = "Shifts retrieved successfully",
                Data = shifts
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shifts");
            return StatusCode(500, new ApiResponse 
            { 
                Success = false,
                Message = "Internal server error occurred while retrieving shifts" 
            });
        }
    }


    [HttpGet("{shiftId:guid}")]
    public async Task<ActionResult<ApiResponse>> GetShiftById(Guid shiftId)
    {
        try
        {
            var shift = await _shiftService.GetShiftById(shiftId);
            if (shift == null)
                return NotFound();

            return Ok(new ApiResponse 
            { 
                Success = true,
                Message = "Shift retrieved successfully",
                Data = shift
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shift with ID: {ShiftId}", shiftId);
            return StatusCode(500, new ApiResponse 
            { 
                Success = false,
                Message = "Internal server error occurred while retrieving shift" 
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> CreateShift([FromBody] CreateShiftDto createshiftDto)
    {
        try
        {
            // Validate model state explicitly
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Invalid shift data",
                    Data = ModelState
                });
            }

            _logger.LogInformation("Controller - object being passed is: {CreateShiftDto}", JsonSerializer.Serialize(createshiftDto, new JsonSerializerOptions { WriteIndented = true }));
            var createdShift = await _shiftService.CreateShift(createshiftDto);

            return CreatedAtAction(
                nameof(GetShiftById),
                new { shiftId = createdShift.ShiftId },
                new ApiResponse 
                { 
                    Success = true,
                    Message = "Shift created successfully",
                    Data = createdShift
                });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Validation error creating shift");
            return BadRequest(new ApiResponse 
            { 
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift");
            return StatusCode(500, new ApiResponse 
            { 
                Success = false,
                Message = "Internal server error occurred while creating shift",
            });
        }
    }
    
    [HttpPut("{shiftId:guid}")]
    public async Task<ActionResult<ApiResponse>> UpdateShift(
        [FromRoute] Guid shiftId, 
        [FromBody] UpdateShiftDto updateShiftDto) 
    {
        try
        {
            if (updateShiftDto == null)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "Update shift data cannot be null"
                });
            }

            var updatedShift = await _shiftService.UpdateShift(shiftId, updateShiftDto);
            if (updatedShift == null)
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = $"Shift with ID {shiftId} not found"
                });

                
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Shift updated successfully",
                Data = updatedShift
            });
        }
        catch (RequestFailedException ex)
        {
            // Log the specific Azure Storage exception
            _logger.LogError(ex, "Azure Storage error updating shift with ID: {ShiftId}. Status Code: {StatusCode}, Error Code: {ErrorCode}", 
                shiftId, ex.Status, ex.ErrorCode);

            return StatusCode(ex.Status, new ApiResponse
            {
                Success = false,
                Message = $"Storage error: {ex.Message}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating shift with ID: {ShiftId}. Full Exception: {ExceptionMessage}", 
                shiftId, ex.Message);

            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = $"Internal server error: {ex.Message}"
            });
        }
    }

    [HttpGet("{shiftId:guid}/clockin")]
    public async Task<ActionResult<ApiResponse>> ClockIn(Guid shiftId)
    {
        try
        {
            // Fetch the existing shift first
            var existingShift = await _shiftService.GetShiftById(shiftId);

            if (existingShift == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = $"Shift with ID {shiftId} not found"
                });
            }

            // Create UpdateShiftDto with only ClockInTime updated
            var updateShiftDto = new UpdateShiftDto
            {
                StartTime = existingShift.StartTime,
                EndTime = existingShift.EndTime,
                ShiftType = existingShift.ShiftType.ToString(),  
                Status = existingShift.Status.ToString(),        
                ClockInTime = DateTime.UtcNow,
                RoleId = existingShift.RoleId
            };

            // Call the existing UpdateShift method to update the ClockInTime
            var updatedShift = await _shiftService.UpdateShift(shiftId, updateShiftDto);

            if (updatedShift == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = $"Shift with ID {shiftId} could not be updated"
                });
            }

            // Create message for RabbitMQ to be published to the clockin queue
            // var clockInMessage = new ClockInDto
            // {
            //     ShiftID = shiftId,
            //     TimeStamp = DateTime.Now,
            //     RoleId = updatedShift.RoleId
            // };

            // // Publish clock-in message to RabbitMQ
            // _rabbitMqProducerService.PublishClockIn(clockInMessage);

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Clock-in successful and message published",
                Data = updatedShift
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clocking in for shift with ID: {ShiftId}", shiftId);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Internal server error occurred while clocking in"
            });
        }
    }


    [HttpDelete("{shiftId:guid}")]
    public async Task<ActionResult> DeleteShift(Guid shiftId)
    {
        try
        {
            var result = await _shiftService.DeleteShift(shiftId);
            if (!result)
            {
                _logger.LogInformation("controller class error");
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shift with ID: {ShiftId}", shiftId);
            return StatusCode(500);
        }
    }

    [HttpGet("{employeeId:guid}/hours")]
    public async Task<ActionResult<ApiResponse>> GetTotalHoursByEmployee(Guid employeeId)
    {
        try
        {
            var totalHours = await _shiftService.GetTotalHoursByEmployee(employeeId);
            return Ok(new ApiResponse 
            { 
                Success = true,
                Message = "Total hours retrieved successfully",
                Data = new { TotalHours = totalHours }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total hours for employee: {EmployeeId}", employeeId);
            return StatusCode(500, new ApiResponse 
            { 
                Success = false,
                Message = "Internal server error occurred while retrieving total hours" 
            });
        }
    }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
}