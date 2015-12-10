﻿namespace Slinqy.Core
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
        /// The name of the queue.
        /// </summary>
        private readonly string queueName;

        /// <summary>
        /// Monitors the physical queue shards.
        /// </summary>
        private readonly SlinqyQueueShardMonitor queueShardMonitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyAgent"/> class.
        /// </summary>
        /// <param name="queueName">
        /// Specifies the name of the virtual queue.
        /// </param>
        /// <param name="queueService">
        /// Specifies the reference to use for managing the queue service.
        /// </param>
        /// <param name="storageCapacityScaleOutThreshold">
        /// Specifies at what percentage of the current physical write queues storage
        /// utilization that the agent should take a scale out action (add another shard).
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was not designed for async calls.")]
        public
        SlinqyAgent(
            string                  queueName,
            IPhysicalQueueService   queueService,
            double                  storageCapacityScaleOutThreshold)
        {
            this.queueName                          = queueName;
            this.queueService                       = queueService;
            this.storageCapacityScaleOutThreshold   = storageCapacityScaleOutThreshold;

            this.queueShardMonitor = new SlinqyQueueShardMonitor(
                this.queueName,
                this.queueService
            );
        }

        /// <summary>
        /// Starts the agent to begin monitoring the queue shards and taking action if needed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "task", Justification = "temp")] // TODO: REMOVE
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
            // Get the write queue shard.
            var writeShard = this.queueShardMonitor.Shards.Single(q => q.Writable);

            // Calculate it's storage utilization.
            // TODO: Move to property on SlinqyQueueShard...?
            var utilizationPercentage = (((double)writeShard.CurrentSizeBytes / 1024) / 1024) / writeShard.MaxSizeMegabytes;

            // Scale up if needed
            if (utilizationPercentage > this.storageCapacityScaleOutThreshold)
                await this.ScaleOut(writeShard).ConfigureAwait(false);
        }

        /// <summary>
        /// Scales out the Slinqy queue by adding a new write shard.
        /// </summary>
        /// <param name="currentWriteShard">
        /// Specifies the current write shard.
        /// A new shard will be added after it, and it then writing will be disabled on it.
        /// </param>
        /// <returns>Returns the async Task for the work.</returns>
        private
        async Task
        ScaleOut(
            SlinqyQueueShard currentWriteShard)
        {
            // Add next shard!
            var nextShardIndex = currentWriteShard.ShardIndex + 1;

            // Create a new shard!
            var newWriteShard = await this.queueService.CreateQueue(currentWriteShard.ShardName + nextShardIndex)
                .ConfigureAwait(false);

            // Set the previous write shards new state.
            await this.SetShardStates();
        }

        /// <summary>
        /// Ensures that all of the shards are in the correct state.
        /// </summary>
        /// <returns>Returns the async Task for the work.</returns>
        private
        Task
        SetShardStates()
        {
            this.ToString();
            return Task.Run(() => { });
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