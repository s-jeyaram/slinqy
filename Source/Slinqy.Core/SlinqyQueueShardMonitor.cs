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
        /// Initializes a new instance of the <see cref="SlinqyQueueShardMonitor"/> class.
        /// </summary>
        /// <param name="queueName">Specifies the name of the virtual queue to monitor.</param>
        /// <param name="queueService">Specifies the queue service hosting the virtual queue.</param>
        public
        SlinqyQueueShardMonitor(
            string                  queueName,
            IPhysicalQueueService   queueService)
        {
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
        /// Gets the current write shard.
        /// </summary>
        public SlinqyQueueShard WriteShard => this.Shards.Last(s => s.PhysicalQueue.Writable);

        /// <summary>
        /// Starts polling the physical resources to update the Shards property values.
        /// </summary>
        /// <returns>Returns the async Task for the work.</returns>
        public
        async Task
        Start()
        {
            // Perform a manual poll now to validate that it works before returning.
            await this.UpdateShards().ConfigureAwait(false);

            // Start polling
            this.pollQueuesTask = this.PollQueues();
            var awaitResult = this.pollQueuesTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the instances Shards collection based on the latest data from the physical queue service.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private
        async Task
        UpdateShards()
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
            while (true)
            {
                await this.UpdateShards().ConfigureAwait(false);
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }
}
