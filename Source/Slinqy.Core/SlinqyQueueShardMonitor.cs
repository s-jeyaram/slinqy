namespace Slinqy.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Monitors the physical shards of a virtual Slinqy queue.
    /// </summary>
    public class SlinqyQueueShardMonitor
    {
        /// <summary>
        /// The physical queue service hosting the Slinqy queue.
        /// </summary>
        private readonly IPhysicalQueueService queueService;

        /// <summary>
        /// The async Task that is running the queue polling operations.
        /// </summary>
        private Task pollQueuesTask;

        /// <summary>
        /// Gets a value indicating whether monitoring is active (true) or not (false).
        /// </summary>
        private bool monitoring;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueShardMonitor"/> class.
        /// </summary>
        /// <param name="queueName">Specifies the name of the virtual queue to monitor.</param>
        /// <param name="queueService">Specifies the queue service hosting the virtual queue.</param>
        public
        SlinqyQueueShardMonitor(
            string                  queueName,
            IPhysicalQueueService   queueService)
        {
            this.monitoring   = false;
            this.QueueName    = queueName;
            this.queueService = queueService;

            this.Shards = Enumerable.Empty<SlinqyQueueShard>();
        }

        /// <summary>
        /// Gets the name of the Slinqy Queue being monitored.
        /// </summary>
        public virtual string QueueName { get; }

        /// <summary>
        /// Gets a list of SlinqyQueueShards for each physical queue found.  This list refreshes periodically.
        /// </summary>
        public virtual IEnumerable<SlinqyQueueShard> Shards { get; private set; }

        /// <summary>
        /// Gets the current shard for sending new queue messages.
        /// </summary>
        public virtual SlinqyQueueShard SendShard => this.Shards.Last(s => s.PhysicalQueue.IsSendEnabled);

        /// <summary>
        /// Gets the current shard for receiving queue messages for processing.
        /// </summary>
        public virtual SlinqyQueueShard ReceiveShard => this.Shards.First(s => s.PhysicalQueue.IsReceiveEnabled);

        /// <summary>
        /// Starts polling the physical resources to update the Shards property values.
        /// </summary>
        public
        virtual
        void
        Start()
        {
            // Start polling
            this.monitoring = true;
            this.pollQueuesTask = this.PollQueues();
            this.pollQueuesTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Stops monitoring.
        /// </summary>
        public
        virtual
        void
        StopMonitoring()
        {
            this.monitoring = false;
        }

        /// <summary>
        /// Updates the instances Shards collection based on the latest data from the physical queue service.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private
        async Task
        Refresh()
        {
            var physicalShards = await this.queueService.ListQueues(this.QueueName)
                .ConfigureAwait(false);

            this.Shards = physicalShards.Select(ps => new SlinqyQueueShard(ps)).ToArray();
        }

        /// <summary>
        /// Periodically retrieves the current state of the queues from the physical queue service.
        /// </summary>
        /// <returns>Returns the async Task for the work.</returns>
        private
        async Task
        PollQueues()
        {
            while (this.monitoring)
            {
                try
                {
                    await this.Refresh().ConfigureAwait(false);
                }
                catch
                {
                    // TODO: Log the exception as a warning.
                }

                // TODO: Make the delay more configurable.
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }
}
