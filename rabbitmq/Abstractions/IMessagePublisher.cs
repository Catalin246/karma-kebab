using System;
using System.Threading.Tasks;

namespace rabbitmq.Abstractions
{
    public interface IMessagePublisher
    {
        /// <summary>
        /// Publishes a message to a specific routing key
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="routingKey">The routing key for the message</param>
        /// <param name="message">The message to publish</param>
        void PublishMessage<T>(string routingKey, T message);

        /// <summary>
        /// Publishes a message asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="routingKey">The routing key for the message</param>
        /// <param name="message">The message to publish</param>
        /// <returns>A task representing the asynchronous publish operation</returns>
        Task PublishMessageAsync<T>(string routingKey, T message);
    }
}