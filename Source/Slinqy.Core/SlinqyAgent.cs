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

            // Determine what the receivable and sendable shards *should* be.
            var sendShard    = queues.Last();
            var receiveShard = queues.FirstOrDefault(q => q.PhysicalQueue.CurrentSizeBytes > 0) ?? sendShard;

            var sendQueue    = sendShard.PhysicalQueue;
            var receiveQueue = receiveShard.PhysicalQueue;

            // If the send and receive queues are not the same, then make sure sending to it is disabled.
            if (receiveQueue != sendQueue && receiveQueue.IsSendEnabled)
                await this.queueService.SetQueueReceiveOnly(receiveQueue.Name);

            // Make sure all shards in between are disabled.
            var inBetweenShards = queues.Where(s =>
                s.PhysicalQueue != receiveQueue &&
                s.PhysicalQueue != sendQueue
            );

            foreach (var shard in inBetweenShards)
            {
                // Ignore shards that are already disabled.
                if (shard.IsDisabled)
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
