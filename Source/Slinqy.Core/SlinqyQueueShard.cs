namespace Slinqy.Core
{
    /// <summary>
    /// Models a phsical queue that is a shard of a larger virtual queue.
    /// </summary>
    public class SlinqyQueueShard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueShard"/> class.
        /// </summary>
        /// <param name="shardName">Specifies the name of the phsyical queue shard.</param>
        /// <param name="maxSizeMegabytes">Specifies the max size of the physical queue shard, in megabytes.</param>
        /// <param name="currentSizeMegabytes">Specifies the current size of the physical queue shard, in megabytes.</param>
        public
        SlinqyQueueShard(
        string  shardName,
        long    maxSizeMegabytes,
        long    currentSizeMegabytes)
        {
            this.ShardName              = shardName;
            this.MaxSizeMegabytes       = maxSizeMegabytes;
            this.CurrentSizeMegabytes   = currentSizeMegabytes;
        }

        /// <summary>
        /// Gets the name for this physical queue shard.
        /// </summary>
        public string ShardName { get; private set; }

        /// <summary>
        /// Gets the maximum capacity for this physical queue shard.
        /// </summary>
        public long MaxSizeMegabytes { get; private set; }

        /// <summary>
        /// Gets the current size of the queue in megabytes.
        /// </summary>
        public long CurrentSizeMegabytes { get; private set; }
    }
}