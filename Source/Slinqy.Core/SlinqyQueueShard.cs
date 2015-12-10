namespace Slinqy.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Models a physical queue that is a shard of a larger virtual queue.
    /// </summary>
    public class SlinqyQueueShard
    {
        private const string ShardIndexRegularExpression = @"\d+$";

        private static readonly Regex ShardIndexRegEx = new Regex(ShardIndexRegularExpression, RegexOptions.Compiled);

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
            if (physicalQueue == null)
                throw new ArgumentNullException(nameof(physicalQueue));

            this.physicalQueue = physicalQueue;

            this.ShardIndex = ParseQueueNameForIndex(this.physicalQueue.Name);
        }

        /// <summary>
        /// Gets the name for this physical queue shard.
        /// </summary>
        public virtual string ShardName => this.physicalQueue.Name;

        /// <summary>
        /// Gets the index of this shard within the SlinqyQueue.
        /// </summary>
        public int ShardIndex { get; }

        /// <summary>
        /// Gets the maximum capacity for this physical queue shard.
        /// </summary>
        public long MaxSizeMegabytes => this.physicalQueue.MaxSizeMegabytes;

        /// <summary>
        /// Gets the current size of the queue in megabytes.
        /// </summary>
        public long CurrentSizeBytes => this.physicalQueue.CurrentSizeBytes;

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

        /// <summary>
        /// Parses the specified name and returns the shard index that is included in the name.
        /// </summary>
        /// <param name="name">
        /// Specifies the name of the physical queue.
        /// </param>
        /// <returns>
        /// Returns the shard index that was parsed from the name.
        /// </returns>
        private
        static
        int
        ParseQueueNameForIndex(
            string name)
        {
            var match = ShardIndexRegEx.Match(name);

            var index = match.Groups[0].Value;

            return int.Parse(index, CultureInfo.InvariantCulture);
        }
    }
}