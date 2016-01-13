namespace ExampleApp.Web.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Slinqy.Core;

    /// <summary>
    /// Models an Azure Service Bus Queue.
    /// </summary>
    public class ServiceBusQueueModel : IPhysicalQueue
    {
        /// <summary>
        /// The client to the Azure Service Bus Queue.
        /// </summary>
        private readonly QueueClient queueClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusQueueModel"/> class.
        /// </summary>
        /// <param name="serviceBusConnectionString">
        /// Specifies the connection string for the Azure Service Bus namespace to use.
        /// </param>
        /// <param name="queueDescription">
        /// Specifies information about the queue.
        /// </param>
        public
        ServiceBusQueueModel(
            string              serviceBusConnectionString,
            QueueDescription    queueDescription)
        {
            this.queueClient = QueueClient.CreateFromConnectionString(
                serviceBusConnectionString,
                queueDescription.Path,
                ReceiveMode.ReceiveAndDelete
            );

            this.Name               = queueDescription.Path;
            this.MaxSizeMegabytes   = queueDescription.MaxSizeInMegabytes;
            this.CurrentSizeBytes   = queueDescription.SizeInBytes;
            this.IsSendEnabled      = new[] {
                                        EntityStatus.Active,
                                        EntityStatus.ReceiveDisabled
                                    }.Any(s => s == queueDescription.Status);
            this.IsReceiveEnabled   = new[] {
                                        EntityStatus.Active,
                                        EntityStatus.SendDisabled
                                    }.Any(s => s == queueDescription.Status);
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the maximum capacity for this Service Bus Queue.
        /// </summary>
        public long MaxSizeMegabytes { get; }

        /// <summary>
        /// Gets the current size, in bytes, that stored messages are consuming.
        /// </summary>
        public long CurrentSizeBytes { get; }

        /// <summary>
        /// Gets a value indicating whether the queue is writable (true) or not (false).
        /// </summary>
        public bool IsSendEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether the queue supports both reading and writing (true) or not (false).
        /// </summary>
        public bool IsReceiveEnabled { get; }

        /// <summary>
        /// Sends a batch of messages to the queue in a single transaction.
        /// </summary>
        /// <param name="batch">
        /// Specifies a batch of messages to be sent.
        /// </param>
        /// <returns>Returns an async Task for the work.</returns>
        public
        async Task
        SendBatch(
            IEnumerable<object> batch)
        {
            await this.queueClient
                .SendBatchAsync(batch.Select(o => new BrokeredMessage(o)))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Receives a batch of messages from the queue in a single transaction.
        /// </summary>
        /// <param name="maxWaitTime">Specifies the maximum amount of time to wait for messages before returning.</param>
        /// <returns>Returns an enumeration of the messages that were received.</returns>
        public
        async Task<IEnumerable<object>>
        ReceiveBatch(
            TimeSpan maxWaitTime)
        {
            var batch = await this.queueClient
                .ReceiveBatchAsync(messageCount: 100, serverWaitTime: maxWaitTime)
                .ConfigureAwait(false);

            return batch.Select(message => (object)message);
        }

        /// <summary>
        /// Sends the message to the queue.
        /// </summary>
        /// <param name="messageBody">Specifies the body of the message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public
        async Task
        Send(
            object messageBody)
        {
            await this.queueClient
                .SendAsync(new BrokeredMessage(messageBody))
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Receives the next message from the queue.
        /// </summary>
        /// <typeparam name="T">Specifies the Type that is expected to return.</typeparam>
        /// <returns>The body of the message that was received.</returns>
        public
        async Task<T>
        Receive<T>()
        {
            var message = await this.queueClient
                .ReceiveAsync()
                .ConfigureAwait(false);

            return message.GetBody<T>();
        }
    }
}