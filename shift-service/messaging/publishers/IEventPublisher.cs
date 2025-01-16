using System;
using System.Threading.Tasks;
using shift_service.messaging.DTOs;
public interface IEventPublisher
{
    Task PublishClockInEvent(ClockInDto clockInDto);
    Task PublishShiftCreatedEvent(ShiftCreatedDto shiftDto);
}