using System;
using System.Threading.Tasks;

namespace rabbitmq.Abstractions
{
    public interface IMessageConsumer
    {
        /// <summary>
        /// Starts consuming messages from a specific queue
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="queue">The queue to consume from</param>
        /// <param name="onMessageReceived">Action to execute when a message is received</param>
        void ConsumeMessage<T>(string queue, Action<T> onMessageReceived);

        /// <summary>
        /// Starts consuming messages from a specific queue asynchronously
        /// </summary>
        /// <typeparam name="T">Type of the message</typeparam>
        /// <param name="queue">The queue to consume from</param>
        /// <param name="onMessageReceived">Async function to execute when a message is received</param>
        /// <returns>A task representing the asynchronous consume operation</returns>
        Task ConsumeMessageAsync<T>(string queue, Func<T, Task> onMessageReceived);
    }
}