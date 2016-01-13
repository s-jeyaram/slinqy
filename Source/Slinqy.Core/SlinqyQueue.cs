namespace Slinqy.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Models a virtual queue that is made up of n number of physical shards.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Queue is not a suffix in this case.")]
    public class SlinqyQueue
    {
        /// <summary>
        /// Used to monitor the physical queue service and report what shards are found.
        /// </summary>
        private readonly SlinqyQueueShardMonitor queueShardMonitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueue"/> class.
        /// </summary>
        /// <param name="slinqyQueueShardMonitor">
        /// Specifies the SlinqyQueueShardMonitor to use for staying in sync with physical queue resources.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was not designed for async calls.")]
        public
        SlinqyQueue(
            SlinqyQueueShardMonitor slinqyQueueShardMonitor)
        {
            this.queueShardMonitor = slinqyQueueShardMonitor;
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        public string Name => this.queueShardMonitor.QueueName;

        /// <summary>
        /// Gets the current maximum storage capacity of the virtual queue.
        /// </summary>
        public long MaxQueueSizeMegabytes => this.queueShardMonitor.Shards.Sum(s => s.PhysicalQueue.MaxSizeMegabytes);

        /// <summary>
        /// Gets the current size of all the data stored in the queue.
        /// </summary>
        public long CurrentQueueSizeBytes => this.queueShardMonitor.Shards.Sum(s => s.PhysicalQueue.CurrentSizeBytes);

        /// <summary>
        /// Sends the specified batch of messages to the current send queue.
        /// </summary>
        /// <param name="batch">Specifies the batch to submit to the send queue.</param>
        /// <returns>Returns the async Task for the work.</returns>
        public
        Task
        SendBatch(
            IEnumerable<object> batch)
        {
            return this.queueShardMonitor.SendShard.SendBatch(batch);
        }

        /// <summary>
        /// Retrieves the next batch of messages from the queue.
        /// </summary>
        /// <param name="maxWaitTime">Specifies the maximum amount of time to wait for messages before returning.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule wasn't designed for async Tasks.")]
        public
        async Task<IEnumerable<object>>
        ReceiveBatch(
            TimeSpan maxWaitTime)
        {
            return await this.queueShardMonitor
                .ReceiveShard
                .ReceiveBatch(maxWaitTime)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the specified message to the current send queue.
        /// </summary>
        /// <param name="messageBody">Specifies the body of the message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public
        async Task
        Send(
            string messageBody)
        {
            await this.queueShardMonitor
                .SendShard
                .Send(messageBody)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Receives the next message from the queue.
        /// </summary>
        /// <typeparam name="T">Specifies the Type that is expected to return.</typeparam>
        /// <returns>Returns the body of the message that was received.</returns>
        public
        async Task<T>
        Receive<T>()
        {
            return await this.queueShardMonitor
                .ReceiveShard
                .Receive<T>();
        }
    }
}