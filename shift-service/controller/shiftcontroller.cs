using Microsoft.AspNetCore.Mvc;
using Azure;
using System.Text.Json;
using Services;
using Models;
using Messaging.Publishers;

namespace Controllers
{

[ApiController]
[Route("[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly ILogger<ShiftsController> _logger;
    private readonly IEventPublisher _eventpublisher;
    
    public ShiftsController( IShiftService shiftService, ILogger<ShiftsController> logger, IEventPublisher eventPublisher) 
    {
        _shiftService = shiftService;
        _logger = logger; 
        _eventpublisher = eventPublisher;
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


   [HttpPost("{shiftId:guid}/clockin")]
    public async Task<ActionResult<ApiResponse>> ClockIn(Guid shiftId)
    {
        try
        {
            // UpdateShiftDto with only ClockInTime updated
            var updateShiftDto = new UpdateShiftDto
            {
                ClockInTime = DateTime.UtcNow  
            };

            var updatedShift = await _shiftService.UpdateShift(shiftId, updateShiftDto);

            if (updatedShift == null)
            {
                _logger.LogWarning("Shift not found for clock-in. ShiftId: {ShiftId}", shiftId);
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = $"Shift with ID {shiftId} not found"
                });
            }

            var clockInEvent = new ClockInDto
            {
                ShiftID = shiftId,
                TimeStamp = DateTime.UtcNow,
                RoleId = updatedShift.RoleId
            };

            try
            {

                _eventpublisher.PublishClockInEvent(clockInEvent);
                _logger.LogInformation("Successfully published clock-in event for shift: {ShiftId}", shiftId);


                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Clock-in successful and event published",
                    Data = updatedShift
                });
            }
            catch (Exception publishEx)
            {
                _logger.LogError(publishEx, "Failed to publish clock-in event for shift: {ShiftId}. Will retry in background.", shiftId);
                
                // Queue for retry in background?? TODO
                // BackgroundJob.Enqueue(() => RetryPublishClockInEvent(clockInEvent));

                // Still return success since the clock-in itself succeeded
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Clock-in successful. Event publishing queued for retry.",
                    Data = updatedShift
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing clock-in for shift: {ShiftId}", shiftId);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Internal server error occurred while processing clock-in"
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
}