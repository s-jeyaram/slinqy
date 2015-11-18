namespace Slinqy.Core
{
    /// <summary>
    /// Models a phsical queue that is a shard of a larger virtual queue.
    /// </summary>
    public class SlinqyQueueShard
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "temp")]
        private string  shardName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueShard"/> class.
        /// </summary>
        /// <param name="shardName">Specifies the name of the phsyical queue shard.</param>
        /// <param name="maxSizeInMegabytes">Specifies the max size of the phsical queue shard.</param>
        public
        SlinqyQueueShard(
        string  shardName,
        long    maxSizeInMegabytes)
        {
            this.shardName          = shardName;
            this.MaxSizeInMegabytes = maxSizeInMegabytes;
        }

        /// <summary>
        /// Gets the maximum capacity for this physical queue shard.
        /// </summary>
        public long MaxSizeInMegabytes { get; private set; }
    }
}