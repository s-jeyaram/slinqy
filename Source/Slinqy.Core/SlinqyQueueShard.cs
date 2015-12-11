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
        /// <summary>
        /// Specifies a regular expression that can extract the index number from a given queue shard name.
        /// This expression will use all numerical characters found at the end of the name as the index value.
        /// For example, all the following should work:
        /// my-queue1234
        /// my-queue-1234
        /// my1queue1234
        /// my1queue-1234
        /// And return 1234 as the index.
        /// </summary>
        private const string ShardIndexRegularExpression = @"\d+$";

        /// <summary>
        /// Instantiates the regular expression as a .NET Regex instance.
        /// </summary>
        private static readonly Regex ShardIndexRegEx = new Regex(ShardIndexRegularExpression, RegexOptions.Compiled);

        /// <summary>
        /// The physical queue underlying this shard.
        /// </summary>
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
        }

        /// <summary>
        /// Gets the name for this physical queue shard.
        /// </summary>
        public virtual string ShardName => this.physicalQueue.Name;

        /// <summary>
        /// Gets the index of this shard within the SlinqyQueue.
        /// </summary>
        public virtual int ShardIndex => ParseQueueNameForIndex(this.physicalQueue.Name);

        /// <summary>
        /// Gets the maximum capacity for this physical queue shard.
        /// </summary>
        public virtual long MaxSizeMegabytes => this.physicalQueue.MaxSizeMegabytes;

        /// <summary>
        /// Gets the current size of the queue in megabytes.
        /// </summary>
        public virtual long CurrentSizeBytes => this.physicalQueue.CurrentSizeBytes;

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
        virtual
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