using System;
using System.Threading.Tasks;
using Azure;

public class ShiftService : IShiftService
{
    private readonly IShiftDbContext _dbContext;
    private readonly ILogger<ShiftService> _logger;

    public ShiftService(IShiftDbContext dbContext, ILogger<ShiftService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ShiftDto> CreateShift(CreateShiftDto createshiftDto)
{
    try 
    {
        if (createshiftDto == null)
            throw new ArgumentNullException(nameof(createshiftDto), "Shift data cannot be null");

        if (!Enum.TryParse<ShiftType>(createshiftDto.ShiftType, out var shiftType))
            throw new ArgumentException("Invalid shift type", nameof(createshiftDto.ShiftType));

        createshiftDto.StartTime = createshiftDto.StartTime.ToUniversalTime();
        createshiftDto.EndTime = createshiftDto.EndTime.ToUniversalTime();

        var shiftEntity = MapToEntity(createshiftDto);

        var savedShift = await _dbContext.AddShift(shiftEntity);

        return ShiftDbContext.MapToDto(savedShift);
    }
    catch (Exception ex) 
    {
        _logger.LogError(ex, "Error creating shift");
        throw; 
    }
}

    public async Task<ShiftDto> GetShiftById(Guid shiftId)
    {
        return await _dbContext.GetShiftById(shiftId);
    }

//this one still needs to be tested (filtering by date for example)
    public async Task<IEnumerable<ShiftDto>> GetShifts(DateTime? date = null, Guid? employeeId = null, ShiftType? shiftType = null, Guid? shiftId = null, Guid? eventId = null)
    {
        var shifts = await _dbContext.GetShifts(date, employeeId, shiftType, shiftId, eventId);
        return ShiftDbContext.MapToDtos(shifts);
    }

    public async Task<ShiftDto> UpdateShift(Guid shiftId, UpdateShiftDto updateShiftDto)
    {
        try 
        {
            // Retrieve the existing entity
            var response = await _dbContext.GetShiftById(shiftId);
            var existingShift = MapToEntity(response);

            if (existingShift == null)
                return null;

            // Retrieve the full entity to get the current ETag
            var tableEntity = await _dbContext.GetShift(existingShift.PartitionKey, existingShift.RowKey);

            var newShift = MapToEntity(updateShiftDto, existingShift);
            // Use the ETag from the retrieved table entity
            newShift.ETag = tableEntity.ETag;

            await _dbContext.UpdateShift(newShift);

            return MapToDto(newShift);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Storage error updating shift with ID: {ShiftId}. Status: {Status}, ErrorCode: {ErrorCode}",
                shiftId, ex.Status, ex.ErrorCode);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating shift with ID: {ShiftId}", shiftId);
            throw;
        }
    }
    public async Task<bool> DeleteShift(Guid shiftId)
        {
            try
            {
                var shiftDto = await _dbContext.GetShiftById(shiftId); 
                if (shiftDto == null) return false;
                var shiftEntity = MapToEntity(shiftDto);
                await _dbContext.DeleteShift(shiftEntity.PartitionKey, shiftEntity.RowKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shift with ID: {ShiftId}", shiftId);
                throw;
            }
        }
    public async Task<decimal> GetTotalHoursByEmployee(Guid employeeId)
    {
        try
        {
            var shifts = await _dbContext.GetShiftsByEmployee(employeeId);
            return shifts.Where(s => s.ClockInTime.HasValue && s.ClockOutTime.HasValue)
                         .Sum(s => (decimal)(s.ClockOutTime.Value - s.ClockInTime.Value).TotalHours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total hours for employee: {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<bool> DeleteEmployeeAndShifts(Guid employeeId)
    {
        try
        {
            var shifts = await _dbContext.GetShiftsByEmployee(employeeId);
            if (!shifts.Any()) return false;

            await _dbContext.DeleteShiftsByEmployee(employeeId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee and shifts for employee ID: {EmployeeId}", employeeId);
            throw;
        }
    }

    // public async Task<IEnumerable<ShiftDto>> UpdateShiftWithEventChanges(Guid eventId, EventDto eventDto) //THIS ONE NEEDS TO BE DISCUSSED
    // {
    //     try
    //     {
    //         var shifts = await _dbContext.GetShifts(null, null, null, null, eventId); 
    //         if (!shifts.Any()) return null;

    //         foreach (var shift in shifts)
    //         {
    //             shift.StartTime = eventDto.StartTime;
    //             shift.EndTime = eventDto.EndTime;
    //             await _dbContext.UpdateShift(shift);  // Update shift in DB
    //         }

    //         return shifts.Select(MapToDto);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Error updating shifts for event: {EventId}", eventId);
    //         throw;
    //     }
    // }
    private static ShiftDto MapToDto(ShiftEntity shift)
{
    return new ShiftDto
    {
        ShiftId = shift.ShiftId,
        StartTime = shift.StartTime,
        EndTime = shift.EndTime,
        EmployeeId = shift.EmployeeId,
        ShiftType = shift.GetShiftTypeEnum(),
        Status = shift.GetStatusEnum(),
        ClockInTime = shift.ClockInTime,
        ClockOutTime = shift.ClockOutTime,
        ShiftHours = shift.ShiftHours
    };
}

    private static ShiftEntity MapToEntity(ShiftDto shiftDto) 
    {
        if (shiftDto == null){
            throw new ArgumentNullException(nameof(shiftDto));
        }

        return new ShiftEntity 
        {
            ShiftId = shiftDto.ShiftId,
            StartTime = shiftDto.StartTime,
            EndTime = shiftDto.EndTime,
            EmployeeId = shiftDto.EmployeeId,
            ShiftType = shiftDto.ShiftType.ToString(),
            Status = shiftDto.Status.ToString(),
            ClockInTime = shiftDto.ClockInTime,
            ClockOutTime = shiftDto.ClockOutTime,
            ShiftHours = shiftDto.ShiftHours.HasValue ? shiftDto.ShiftHours.Value : (decimal?)null
        };
    }
    private static ShiftEntity MapToEntity(CreateShiftDto createShiftDto)
    {
        if (createShiftDto == null){
            throw new ArgumentNullException(nameof(createShiftDto));
        }
        var newShiftId = Guid.NewGuid();

        return new ShiftEntity 
        {
            ShiftId = newShiftId,
            EmployeeId = createShiftDto.EmployeeId,
            StartTime = createShiftDto.StartTime,
            EndTime = createShiftDto.EndTime,
            ShiftType = createShiftDto.ShiftType, 
            Status = ShiftStatus.Unconfirmed.ToString(), // Default status
            ClockInTime = null,
            ClockOutTime = null,
            ShiftHours = null
        };
    }
private static ShiftEntity MapToEntity(UpdateShiftDto updateShiftDto, ShiftEntity existingEntity)
{
    if (existingEntity == null)
        throw new ArgumentNullException(nameof(existingEntity), "Existing shift entity must be provided");

    // Specify UTC Kind for all DateTime properties 
    existingEntity.StartTime = updateShiftDto.StartTime.Kind == DateTimeKind.Unspecified 
        ? DateTime.SpecifyKind(updateShiftDto.StartTime, DateTimeKind.Utc) 
        : updateShiftDto.StartTime;

    existingEntity.EndTime = updateShiftDto.EndTime.Kind == DateTimeKind.Unspecified 
        ? DateTime.SpecifyKind(updateShiftDto.EndTime, DateTimeKind.Utc) 
        : updateShiftDto.EndTime;

    existingEntity.ShiftType = updateShiftDto.ShiftType.ToString();
    
    if (updateShiftDto.Status != default)
    {
        existingEntity.Status = updateShiftDto.Status.ToString();
    }
    existingEntity.ClockInTime = updateShiftDto.ClockInTime.HasValue
        ? (updateShiftDto.ClockInTime.Value.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(updateShiftDto.ClockInTime.Value, DateTimeKind.Utc) 
            : updateShiftDto.ClockInTime.Value)
        : (DateTime?)null;

    existingEntity.ClockOutTime = updateShiftDto.ClockOutTime.HasValue
        ? (updateShiftDto.ClockOutTime.Value.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(updateShiftDto.ClockOutTime.Value, DateTimeKind.Utc) 
            : updateShiftDto.ClockOutTime.Value)
        : (DateTime?)null;

    return existingEntity;
}

}

