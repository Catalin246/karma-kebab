using Models;
using System.Threading.Tasks;

namespace Services
{
    public interface IRabbitMqService
    {
        Task PublishClockInEvent(ClockInDto clockInDto);
        Task PublishShiftCreatedEvent(ShiftCreatedDto shiftDto);
        Task StartSubscribers();
        Task StartEventCreatedSubscriber();
        Task StartEventDeletedSubscriber();
    }
}
