using System.Threading.Tasks;

namespace Services
{
    public interface IRabbitMqService
    {
        Task ListeningEventCreated();

        Task PublishClockIn(ClockInDto clockInDto);
    }
}
