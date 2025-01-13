public interface IRabbitMqProducerService
{
    Task PublishClockIn(ClockInDto clockInDto);
    Task PublishShiftCreated();
}
