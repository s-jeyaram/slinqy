namespace Slinqy.Core
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Monitors the physical shards of a virtual Slinqy queue.
    /// </summary>
    public class SlinqyQueueShardMonitor
    {
        private string                  queueName;
        private IPhysicalQueueService   queueService;
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Temp")] // TODO: REMOVE
        private Task                    pollQueuesTask;

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
            this.queueName    = queueName;
            this.queueService = queueService;

            this.Shards = Enumerable.Empty<SlinqyQueueShard>();
        }

        /// <summary>
        /// Gets a list of SlinqyQueueShards for each physical queue found.  This list refreshes periodically.
        /// </summary>
        public IEnumerable<SlinqyQueueShard> Shards { get; private set; }

        /// <summary>
        /// Gets the current write shard.
        /// </summary>
        public SlinqyQueueShard WriteShard
        {
            get { return this.Shards.Single(s => s.Writable); }
        }

        /// <summary>
        /// Starts polling the physical resources to update the Shards property values.
        /// </summary>
        /// <returns>Returns the async Task for the work.</returns>
        public
        async Task
        Start()
        {
            // Perform a manual poll now to validate that it works before returning.
            await this.UpdateShards();

            // Start polling
            this.pollQueuesTask = this.PollQueues();
        }

        private
        async Task
        UpdateShards()
        {
            this.Shards = await this.queueService.ListQueues(this.queueName);
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
                await this.UpdateShards();

                await Task.Delay(1000);
            }
        }
    }
}
