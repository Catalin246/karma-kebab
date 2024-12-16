using System.Threading.Tasks;

namespace Services
{
    public interface IRabbitMqService
    {
        Task StartListeningAsync();
    }
}
