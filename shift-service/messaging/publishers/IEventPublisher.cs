using System;
using System.Threading.Tasks;
using Models;

namespace Messaging.Publishers {
public interface IEventPublisher
{
    Task PublishClockInEvent(ClockInDto clockInDto);
    Task PublishShiftCreatedEvent(ShiftDto shiftDto);
    Task PublishShiftDeletedEvent(Guid shiftID);
}}