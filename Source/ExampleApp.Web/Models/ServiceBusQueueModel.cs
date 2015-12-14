namespace ExampleApp.Web.Models
{
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
                queueDescription.Path
            );

            this.Name               = queueDescription.Path;
            this.MaxSizeMegabytes   = queueDescription.MaxSizeInMegabytes;
            this.CurrentSizeBytes   = queueDescription.SizeInBytes;
            this.Writable           = new[] {
                                        EntityStatus.Active,
                                        EntityStatus.ReceiveDisabled
                                    }.Any(s => s == queueDescription.Status);
            this.ReadWritable       = queueDescription.Status == EntityStatus.Active;
            this.Disabled           = queueDescription.Status == EntityStatus.Disabled;
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
        public bool Writable { get; }

        /// <summary>
        /// Gets a value indicating whether the queue supports both reading and writing (true) or not (false).
        /// </summary>
        public bool ReadWritable { get; }

        /// <summary>
        /// Gets a value indicating whether the queue is disabled for both reading and writing (true), or not (false).
        /// </summary>
        public bool Disabled { get; }

        /// <summary>
        /// Sends a batch of messages to the queue in a single transaction.
        /// </summary>
        /// <param name="batch">
        /// Specifies a batch of messages to be sent.
        /// </param>
        /// <returns>Returns an async Task for the work.</returns>
        public
        Task
        SendBatch(
            IEnumerable<object> batch)
        {
            return this.queueClient.SendBatchAsync(
                batch.Select(o => new BrokeredMessage(o))
            );
        }
    }
}