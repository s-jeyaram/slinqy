namespace Slinqy.Core
{
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
        public long MaxQueueSizeMegabytes => this.queueShardMonitor.Shards.Sum(s => s.MaxSizeMegabytes);

        /// <summary>
        /// Gets the current size of all the data stored in the queue.
        /// </summary>
        public long CurrentQueueSizeBytes => this.queueShardMonitor.Shards.Sum(s => s.CurrentSizeBytes);

        /// <summary>
        /// Sends the specified batch of messages to the current write queue.
        /// </summary>
        /// <param name="batch">Specifies the batch to submit to the write queue.</param>
        /// <returns>Returns the async Task for the work.</returns>
        public
        Task
        SendBatch(
            IEnumerable<object> batch)
        {
            return this.queueShardMonitor.WriteShard.SendBatch(batch);
        }
    }
}