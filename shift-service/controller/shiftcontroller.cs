using Microsoft.AspNetCore.Mvc;
using Azure;
using System.Text.Json;
using Services;

[ApiController]
[Route("[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly ILogger<ShiftsController> _logger;
    private readonly IRabbitMqService _rabbitMqService;

    public ShiftsController(IShiftService shiftService, ILogger<ShiftsController> logger,IRabbitMqService rabbitMqService)  
    {
        _shiftService = shiftService;
        _logger = logger;
        _rabbitMqService = rabbitMqService;  
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


   [HttpGet("clock/{shiftId:guid}")]
    public async Task<ActionResult<ApiResponse>> ClockInOut([FromRoute] Guid shiftId)
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            
            // Get the current shift first to check its state
            var currentShift = await _shiftService.GetShiftById(shiftId)
                .WaitAsync(cts.Token);
            
        _logger.LogInformation("Fetching shift with ID: {ShiftId}", shiftId);
            var currentShift = await _shiftService.GetShiftById(shiftId);
            if (currentShift == null)
            {
                _logger.LogWarning("Shift with ID: {ShiftId} not found", shiftId);
            }

            var currentTime = DateTime.Now;
            var isClockIn = !currentShift.ClockInTime.HasValue;
            _logger.LogInformation("shift clocking controller")
            var updateShiftDto = new UpdateShiftDto
            {
                StartTime = currentShift.StartTime, // Preserve the existing start time
                EndTime = currentShift.EndTime,     // Preserve the existing end time
                ShiftType = currentShift.ShiftType, // Preserve the existing shift type
                Status = currentShift.Status, // Update status
                ClockInTime = isClockIn ? currentTime : currentShift.ClockInTime, // Set clock-in time if clocking in
                ClockOutTime = !isClockIn ? currentTime : currentShift.ClockOutTime, // Set clock-out time if clocking out
                RoleId = currentShift.RoleId // Preserve the existing role ID
            };
    }


            // Call the existing UpdateShift method with timeout
            var updatedShift = await _shiftService.UpdateShift(shiftId, updateShiftDto)
                .WaitAsync(cts.Token);

            if (isClockIn && _rabbitMqService != null)
            {
                var clockMessage = new ClockInDto
                {
                    ShiftID = shiftId,
                    TimeStamp = currentTime,
                    RoleId = updatedShift.RoleId
                };
                
                try
                {
                    _rabbitMqService.PublishClockIn(clockMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish clock message to RabbitMQ, but shift was updated");
                    // Continue execution since the shift update was successful
                }
            }

            var operationType = isClockIn ? "Clock-in" : "Clock-out";
            return Ok(new ApiResponse
            {
                Success = true,
                Message = $"{operationType} successful" + (isClockIn ? " and message published" : ""),
                Data = updatedShift
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(503, new ApiResponse
            {
                Success = false,
                Message = "The operation timed out. Please try again."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing clock in/out for shift with ID: {ShiftId}", shiftId);
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = "Internal server error occurred while processing clock in/out"
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