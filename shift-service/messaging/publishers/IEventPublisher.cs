using System;
using System.Threading.Tasks;

namespace Messaging.Publishers {
public interface IEventPublisher
{
    public void PublishClockInEvent(ClockInDto clockInDto);
    public void PublishShiftCreatedEvent(ShiftCreatedDto shiftDto);
}}