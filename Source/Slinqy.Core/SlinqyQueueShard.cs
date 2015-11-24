namespace Slinqy.Core
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Models a physical queue that is a shard of a larger virtual queue.
    /// </summary>
    public class SlinqyQueueShard
    {
        private readonly IPhysicalQueue physicalQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueShard"/> class.
        /// </summary>
        /// <param name="physicalQueue">
        /// Specifies the actual technology specific implementation of the queue.
        /// </param>
        public
        SlinqyQueueShard(
            IPhysicalQueue physicalQueue)
        {
            this.physicalQueue = physicalQueue;
        }

        /// <summary>
        /// Gets the name for this physical queue shard.
        /// </summary>
        public virtual string ShardName => this.physicalQueue.Name;

        /// <summary>
        /// Gets the index of this shard relative to the other shards in the same SlinqyQueue.
        /// </summary>
        public int ShardIndex { get; set; }

        /// <summary>
        /// Gets the maximum capacity for this physical queue shard.
        /// </summary>
        public long MaxSizeMegabytes => this.physicalQueue.MaxSizeMegabytes;

        /// <summary>
        /// Gets the current size of the queue in megabytes.
        /// </summary>
        public double CurrentSizeBytes => this.physicalQueue.CurrentSizeBytes;

        /// <summary>
        /// Gets a boolean value to indicate if the shard is writable (true) or not (false).
        /// </summary>
        public virtual bool Writable => this.physicalQueue.Writable;

        /// <summary>
        /// Sends the batch of messages to the physical queue shard.
        /// </summary>
        /// <param name="batch">
        /// Specifies the messages to send.
        /// </param>
        /// <returns>Returns an async Task for the work.</returns>
        public
        Task
        SendBatch(
            IEnumerable<object> batch)
        {
            return this.physicalQueue.SendBatch(batch);
        }
    }
}