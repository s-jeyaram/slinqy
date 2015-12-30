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
        public virtual int ShardIndex
        {
            get
            {
                var index   = 0;
                var padding = 0;

                ParseQueueName(
                    this.PhysicalQueue.Name,
                    out index,
                    out padding
                );

                return index;
            }
        }

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
        /// Generates the name of the first physical queue shard for a Slinqy queue.
        /// </summary>
        /// <param name="slinqyQueueName">
        /// Specifies the name of the Slinqy queue.
        /// </param>
        /// <param name="shardIndexPadding">
        /// Specifies how many digits the shard index can occupy in the physical queue name.
        /// </param>
        /// <returns>
        /// Returns the name of what the first physical queue shard should be.
        /// </returns>
        public
        static
        string
        GenerateFirstShardName(
            string  slinqyQueueName,
            int     shardIndexPadding)
        {
            return slinqyQueueName + 0.ToString("D" + shardIndexPadding, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses the specified shard name and generates the next one based on it.
        /// </summary>
        /// <param name="shardName">
        /// Specifies the name of a shard to parse.  Any zero padding will be maintained.
        /// </param>
        /// <returns>Returns the next shard name based on the specified shard name.</returns>
        public
        static
        string
        GenerateNextShardName(
            string shardName)
        {
            var index   = 0;
            var padding = 0;

            var slinqyQueueName = ParseQueueName(
                shardName,
                out index,
                out padding
            );

            var nextIndex            = index + 1;
            var nextIndexWithPadding = nextIndex.ToString("D" + padding, CultureInfo.InvariantCulture);

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}{1}",
                slinqyQueueName,
                nextIndexWithPadding
            );
        }

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
        /// Sends the specified message body to the queue.
        /// </summary>
        /// <param name="messageBody">Specifies the body of the message to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public
        virtual
        async Task
        Send(
            string messageBody)
        {
            await this.PhysicalQueue
                .Send(messageBody)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Receives the next message from the queue.
        /// </summary>
        /// <returns>Returns the body of the message that was received.</returns>
        public
        virtual
        async Task<string>
        Receive()
        {
            return await this.PhysicalQueue
                .Receive()
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Parses the specified name and returns the Slinqy Queue name.
        /// </summary>
        /// <param name="name">
        /// Specifies the name of the physical queue.
        /// </param>
        /// <param name="index">
        /// Returns the numerical index of the shard based on the name.
        /// </param>
        /// <param name="padding">
        /// Returns the amount of padding that was included in the name.
        /// </param>
        /// <returns>
        /// Returns the shard index that was parsed from the name.
        /// </returns>
        private
        static
        string
        ParseQueueName(
            string  name,
            out int index,
            out int padding)
        {
            // Parse the name to extract the index using a regular expression.
            var match = ShardIndexRegEx.Match(name);

            // Get the full index string, including any zero padding.
            var indexString = match.Groups[0].Value;

            // Parse out just the name of the queue, minus any index information.
            var parsedName  = name.Substring(0, name.Length - indexString.Length);

            // Parse the index to an actual integer.
            index   = int.Parse(indexString, CultureInfo.InvariantCulture);
            padding = indexString.Length;

            return parsedName;
        }
    }
}