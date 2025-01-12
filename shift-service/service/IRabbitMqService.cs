using System.Threading.Tasks;

namespace Services
{
    public interface IRabbitMqService
    {
        Task PublishClockIn(ClockInDto clockInDto);
        Task ListeningEventCreated();
        Task ListeningEventDeleted();
    }
}
