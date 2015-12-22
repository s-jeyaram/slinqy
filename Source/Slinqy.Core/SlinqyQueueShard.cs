namespace Slinqy.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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

            this.PhysicalQueue = physicalQueue;
        }

        /// <summary>
        /// Gets the IPhysicalQueue underlying this shard.
        /// </summary>
        public virtual IPhysicalQueue PhysicalQueue { get; }

        /// <summary>
        /// Gets the index of this shard within the SlinqyQueue.
        /// </summary>
        public virtual int ShardIndex => ParseQueueNameForIndex(this.PhysicalQueue.Name);

        /// <summary>
        /// Gets the current storage utilization percentage of this shard.
        /// </summary>
        public virtual double StorageUtilization => (((double)this.PhysicalQueue.CurrentSizeBytes / 1024) / 1024) / this.PhysicalQueue.MaxSizeMegabytes;

        /// <summary>
        /// Gets a value indicating whether both sending and receiving are disabled on this queue.
        /// </summary>
        public virtual bool IsDisabled => !this.PhysicalQueue.IsReceiveEnabled && !this.PhysicalQueue.IsSendEnabled;

        /// <summary>
        /// Gets a value indicating whether receiving is the only operation enabled (true), or not (false).
        /// </summary>
        public virtual bool IsReceiveOnly => this.PhysicalQueue.IsReceiveEnabled && !this.PhysicalQueue.IsSendEnabled;

        /// <summary>
        /// Gets a value indicating whether this shard is enabled for both sending and receiving (true), or not (false).
        /// </summary>
        public virtual bool IsSendReceiveEnabled => this.PhysicalQueue.IsReceiveEnabled && this.PhysicalQueue.IsSendEnabled;

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
            return this.PhysicalQueue.SendBatch(batch);
        }

        /// <summary>
        /// Receives a batch of messages from the queue.
        /// </summary>
        /// <param name="maxWaitTime">Specifies the maximum amount of time to wait for messages before returning.</param>
        /// <returns>Returns an enumeration of messages that were received.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule wasn't designed for async Tasks.")]
        public
        virtual
        async Task<IEnumerable<object>>
        ReceiveBatch(
            TimeSpan maxWaitTime)
        {
            return await this.PhysicalQueue
                .ReceiveBatch(maxWaitTime)
                .ConfigureAwait(false);
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