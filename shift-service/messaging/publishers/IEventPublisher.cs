using System;
using System.Threading.Tasks;

namespace Messaging.Publishers {
public interface IEventPublisher
{
    Task PublishClockInEvent(ClockInDto clockInDto);
    Task PublishShiftCreatedEvent(ShiftCreatedDto shiftDto);
}}