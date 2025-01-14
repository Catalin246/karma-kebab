using System;
using System.Threading.Tasks;
using Azure;
using Models;
using Services;

public class ShiftService : IShiftService
{
    private readonly IShiftDbContext _dbContext;
    private readonly ILogger<ShiftService> _logger;

    private readonly IRabbitMqProducerService _rabbitMqProducerService;

    public ShiftService(IShiftDbContext dbContext, ILogger<ShiftService> logger, IRabbitMqProducerService rabbitMqProducerService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _rabbitMqProducerService = rabbitMqProducerService;
    }

    public async Task<ShiftDto> CreateShift(CreateShiftDto createshiftDto)
    {
        try
        {
            if (createshiftDto == null)
                throw new ArgumentNullException(nameof(createshiftDto), "Shift data cannot be null");

            if (!Enum.TryParse<ShiftType>(createshiftDto.ShiftType.ToString(), out var shiftType))
                throw new ArgumentException("Invalid shift type", nameof(createshiftDto.ShiftType));

            createshiftDto.StartTime = createshiftDto.StartTime.ToUniversalTime();
            createshiftDto.EndTime = createshiftDto.EndTime.ToUniversalTime();
             
            string uuidString = createshiftDto.RowKey;
            Guid rowKey = Guid.Parse(uuidString);
            string partitionKey = createshiftDto.PartitionKey;

            var shiftEntity = MapToEntity(createshiftDto);

            var savedShift = await _dbContext.AddShift(shiftEntity);

            // Prepare the shift created message
            ShiftCreatedMessage shiftCreatedMessage = new ShiftCreatedMessage(
                savedShift.ShiftId,                           
                rowKey,                                           
                partitionKey                                  
            );

            // Call the method to publish shift created message, passing the shift created data
            await _rabbitMqProducerService.PublishShiftCreated(shiftCreatedMessage);

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

    public async Task<IEnumerable<ShiftDto>> GetShifts(DateTime? startDate = null, DateTime? endDate = null, Guid? employeeId = null, ShiftType? shiftType = null, Guid? shiftId = null, Guid? eventId = null)
    {
        var shifts = await _dbContext.GetShifts(startDate, endDate, employeeId, shiftType, shiftId, eventId);
        return ShiftDbContext.MapToDtos(shifts);
    }

    public async Task<ShiftDto> UpdateShift(Guid shiftId, UpdateShiftDto updateShiftDto)
    {
        try
        {
            var response = await _dbContext.GetShiftById(shiftId);
            var existingShift = MapToEntity(response);

            if (existingShift == null)
                return null;

            var tableEntity = await _dbContext.GetShift(existingShift.PartitionKey, existingShift.RowKey);

            var newShift = MapToEntity(updateShiftDto, existingShift);  
            newShift.ETag = tableEntity.ETag;

            await _dbContext.UpdateShift(newShift);

            return MapToDto(newShift);
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
            ShiftHours = shift.ShiftHours,
            RoleId = shift.RoleId
        };
    }

    private static ShiftEntity MapToEntity(ShiftDto shiftDto)
    {
        if (shiftDto == null)
        {
            throw new ArgumentNullException(nameof(shiftDto));
        }

        return new ShiftEntity
        {
            ShiftId = shiftDto.ShiftId,
            StartTime = shiftDto.StartTime,
            EndTime = shiftDto.EndTime,
            EmployeeId = shiftDto.EmployeeId ?? Guid.Empty,
            ShiftType = shiftDto.ShiftType.ToString(),
            Status = shiftDto.Status.ToString(),
            ClockInTime = shiftDto.ClockInTime,
            ClockOutTime = shiftDto.ClockOutTime,
            ShiftHours = shiftDto.ShiftHours.HasValue ? shiftDto.ShiftHours.Value : (decimal?)null,
            RoleId = shiftDto.RoleId
        };
    }
    private static ShiftEntity MapToEntity(CreateShiftDto createShiftDto)
    {
        if (createShiftDto == null)
        {
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
            ShiftHours = null,
            RoleId = createShiftDto.RoleId
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

        existingEntity.RoleId = updateShiftDto.RoleId;
        return existingEntity;
    }

}
