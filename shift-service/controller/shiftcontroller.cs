using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("[controller]")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _shiftService;
    private readonly ILogger<ShiftsController> _logger;

    public ShiftsController(IShiftService shiftService, ILogger<ShiftsController> logger)
    {
        _shiftService = shiftService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse>> ListShifts(
        [FromQuery] DateTime? date = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] ShiftType? shiftType = null,
        [FromQuery] Guid? shiftId = null)
    {
        try
        {
            var shifts = await _shiftService.GetShiftsAsync(date, employeeId, shiftType, shiftId);
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
            var shift = await _shiftService.GetShiftByIdAsync(shiftId);
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
    public async Task<ActionResult<ApiResponse>> CreateShift([FromBody] ShiftDto shiftDto)
    {
        try
        {
            var createdShift = await _shiftService.CreateShiftAsync(shiftDto);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating shift");
            return StatusCode(500, new ApiResponse 
            { 
                Success = false,
                Message = "Internal server error occurred while creating shift" 
            });
        }
    }

    [HttpPut("{shiftId:guid}")]
    public async Task<ActionResult<ApiResponse>> UpdateShift(Guid shiftId, [FromBody] ShiftDto shiftDto)
    {
        try
        {
            var updatedShift = await _shiftService.UpdateShiftAsync(shiftId, shiftDto);
            if (updatedShift == null)
                return NotFound();

            return Ok(new ApiResponse 
            { 
                Success = true,
                Message = "Shift updated successfully",
                Data = updatedShift
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shift with ID: {ShiftId}", shiftId);
            return StatusCode(500, new ApiResponse 
            { 
                Success = false,
                Message = "Internal server error occurred while updating shift" 
            });
        }
    }

    [HttpDelete("{shiftId:guid}")]
    public async Task<ActionResult> DeleteShift(Guid shiftId)
    {
        try
        {
            var result = await _shiftService.DeleteShiftAsync(shiftId);
            if (!result)
                return NotFound();

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
            var totalHours = await _shiftService.GetTotalHoursByEmployeeAsync(employeeId);
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

    [HttpDelete("delete/{employeeId:guid}")]
    public async Task<ActionResult> DeleteEmployeeAndShifts(Guid employeeId)
    {
        try
        {
            var result = await _shiftService.DeleteEmployeeAndShiftsAsync(employeeId);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee and shifts for employee ID: {EmployeeId}", employeeId);
            return StatusCode(500);
        }
    }

    [HttpPut("/events/{eventId:guid}")]
    public async Task<ActionResult<ApiResponse>> UpdateShiftWithEventChanges(Guid eventId, [FromBody] EventDto eventDto)
    {
        try
        {
            var updatedShifts = await _shiftService.UpdateShiftWithEventChangesAsync(eventId, eventDto);
            if (updatedShifts == null)
                return NotFound();

            return Ok(new ApiResponse 
            { 
                Success = true,
                Message = "Shifts updated successfully based on event changes",
                Data = updatedShifts
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating shifts for event: {EventId}", eventId);
            return StatusCode(500, new ApiResponse 
            { 
                Success = false,
                Message = "Internal server error occurred while updating shifts for event" 
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