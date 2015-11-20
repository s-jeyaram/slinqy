namespace Slinqy.Core
{
    /// <summary>
    /// Models a physical queue that is a shard of a larger virtual queue.
    /// </summary>
    public class SlinqyQueueShard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueShard"/> class.
        /// </summary>
        /// <param name="shardName">Specifies the name of the physical queue shard.</param>
        /// <param name="shardIndex">Specifies the index number of this shard.</param>
        /// <param name="maxSizeMegabytes">Specifies the max size of the physical queue shard, in megabytes.</param>
        /// <param name="currentSizeMegabytes">Specifies the current size of the physical queue shard, in megabytes.</param>
        /// <param name="writable">Specifies if the queue is writable.</param>
        public
        SlinqyQueueShard(
        string  shardName,
        int     shardIndex,
        long    maxSizeMegabytes,
        double  currentSizeMegabytes,
        bool    writable)
        {
            this.ShardName              = shardName;
            this.ShardIndex             = shardIndex;
            this.MaxSizeMegabytes       = maxSizeMegabytes;
            this.CurrentSizeMegabytes   = currentSizeMegabytes;
            this.Writable               = writable;
        }

        /// <summary>
        /// Gets the name for this physical queue shard.
        /// </summary>
        public virtual string ShardName { get; private set; }

        /// <summary>
        /// Gets the index of this shard relative to the other shards in the same SlinqyQueue.
        /// </summary>
        public int ShardIndex { get; set; }

        /// <summary>
        /// Gets the maximum capacity for this physical queue shard.
        /// </summary>
        public long MaxSizeMegabytes { get; private set; }

        /// <summary>
        /// Gets the current size of the queue in megabytes.
        /// </summary>
        public double CurrentSizeMegabytes { get; private set; }

        /// <summary>
        /// Gets a boolean value to indicate if the shard is writable (true) or not (false).
        /// </summary>
        public bool Writable { get; private set; }
    }
}