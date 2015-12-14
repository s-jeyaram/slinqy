namespace Slinqy.Core
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// The main class responsible for monitoring and managing queue resources.
    /// </summary>
    public class SlinqyAgent
    {
        /// <summary>
        /// The threshold at which the agent should scale out storage capacity.
        /// </summary>
        private readonly double storageCapacityScaleOutThreshold;

        /// <summary>
        /// The reference for managing the queue service to do things like create new queues.
        /// </summary>
        private readonly IPhysicalQueueService queueService;

        /// <summary>
        /// Monitors the physical queue shards.
        /// </summary>
        private readonly SlinqyQueueShardMonitor queueShardMonitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyAgent"/> class.
        /// </summary>
        /// <param name="queueService">
        /// Specifies the reference to use for managing the queue service.
        /// </param>
        /// <param name="slinqyQueueShardMonitor">
        /// Specifies the monitor of the queue shards.
        /// </param>
        /// <param name="storageCapacityScaleOutThreshold">
        /// Specifies at what percentage of the current physical write queues storage
        /// utilization that the agent should take a scale out action (add another shard).
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was not designed for async calls.")]
        public
        SlinqyAgent(
            IPhysicalQueueService   queueService,
            SlinqyQueueShardMonitor slinqyQueueShardMonitor,
            double                  storageCapacityScaleOutThreshold)
        {
            this.queueService                       = queueService;
            this.queueShardMonitor                  = slinqyQueueShardMonitor;
            this.storageCapacityScaleOutThreshold   = storageCapacityScaleOutThreshold;
        }

        /// <summary>
        /// Starts the agent to begin monitoring the queue shards and taking action if needed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public
        async Task
        Start()
        {
            // Start the monitor.
            await this.queueShardMonitor.Start().ConfigureAwait(false);

            // Perform a manual first check before returning.
            await this.EvaluateShards().ConfigureAwait(false);

            // Start checking the monitor periodically to respond if need be.
            var task = this.PollShardState().ConfigureAwait(false);
        }

        /// <summary>
        /// Checks the current state of the shards and responds if needed.
        /// </summary>
        /// <returns>Returns the async Task for the work.</returns>
        private
        async Task
        EvaluateShards()
        {
            // Get the send queue shard.
            var sendShard = this.queueShardMonitor.SendShard;

            // Scale if needed.
            if (sendShard.StorageUtilization > this.storageCapacityScaleOutThreshold)
                await this.ScaleOut(sendShard).ConfigureAwait(false);

            // Make sure shard states are set properly.
            await this.SetShardStates();
        }

        /// <summary>
        /// Scales out the Slinqy queue by adding a new send shard.
        /// </summary>
        /// <param name="currentSendShard">
        /// Specifies the current send shard.
        /// A new shard will be added after it, and it then sending will be disabled on it.
        /// </param>
        /// <returns>Returns the async Task for the work.</returns>
        private
        async Task
        ScaleOut(
            SlinqyQueueShard currentSendShard)
        {
            // Add next shard!
            var nextShardIndex = currentSendShard.ShardIndex + 1;
            var nextShardName  = this.queueShardMonitor.QueueName + nextShardIndex;

            await this.queueService.CreateSendOnlyQueue(nextShardName)
                .ConfigureAwait(false);

            // Proactively tell the monitor to refresh so it sees the new shard immediately.
            await this.queueShardMonitor.Refresh();
        }

        /// <summary>
        /// Ensures that all of the shards are in the correct state.
        /// </summary>
        /// <returns>Returns the async Task for the work.</returns>
        private
        async Task
        SetShardStates()
        {
            // Get latest info
            var queues = this.queueShardMonitor.Shards.ToArray();

            // Determine what the readable and writable shards should be.
            var lastSendableShard  = queues.Last();
            var firstReadableShard = queues.FirstOrDefault(q => q.PhysicalQueue.CurrentSizeBytes > 0) ?? lastSendableShard;

            var lastSendableQueue  = lastSendableShard.PhysicalQueue;
            var firstReadableQueue = firstReadableShard.PhysicalQueue;

            // If the first and last queue are the same, make sure it's fully enabled.
            if (firstReadableQueue == lastSendableQueue && !firstReadableQueue.ReadWritable)
            {
                // Set the shard to its proper state.
                await this.queueService.SetQueueEnabled(firstReadableQueue.Name);

                // Return now since there's only one shard and it's been set.
                return;
            }

            // If the first read queue is not the same as the last, then make sure sending to it is disabled.
            if (firstReadableQueue != lastSendableQueue && firstReadableQueue.IsSendEnabled)
                await this.queueService.SetQueueReceiveOnly(firstReadableQueue.Name);

            // Make sure all shards in between are disabled.
            var inBetweenShards = queues.Where(s =>
                s.PhysicalQueue != firstReadableQueue ||
                s.PhysicalQueue != lastSendableQueue
            );

            foreach (var shard in inBetweenShards)
            {
                // Ignore shards that are already disabled.
                if (shard.PhysicalQueue.Disabled)
                    continue;

                // Disable it!
                await this.queueService.SetQueueDisabled(shard.PhysicalQueue.Name);
            }
        }

        /// <summary>
        /// Periodically evaluates the shards.
        /// </summary>
        /// <returns>Returns the async Task for the work.</returns>
        private
        async Task
        PollShardState()
        {
            while (true)
            {
                // TODO: Add try/catch block and log exceptions so that exceptions don't stop the agent from polling.

                // Evaluate the current state.
                await this.EvaluateShards().ConfigureAwait(false);

                // Wait before checking again.
                // TODO: Make duration more configurable.
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }
}
