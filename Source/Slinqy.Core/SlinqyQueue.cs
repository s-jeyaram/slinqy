namespace Slinqy.Core
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Models a virtual queue that is made up of n number of physical shards.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Queue is not a suffix in this case.")]
    public class SlinqyQueue
    {
        /// <summary>
        /// Used to monitor the physical queue service and report what shards are found.
        /// </summary>
        private readonly SlinqyQueueShardMonitor queueShardMonitor;

        /// <summary>
        /// Used to manage the physical queue resources.
        /// </summary>
        private IPhysicalQueueService physicalQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueue"/> class.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="physicalQueueService">
        /// Specifies the IPhsicalQueueService to use for managing queue resources.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was not designed for async calls.")]
        public
        SlinqyQueue(
            string                  queueName,
            IPhysicalQueueService   physicalQueueService)
        {
            // Save the service reference.
            this.physicalQueueService = physicalQueueService;

            // Save the queue name.
            this.Name = queueName;

            // Create a queue monitor.
            this.queueShardMonitor = new SlinqyQueueShardMonitor(
                queueName,
                this.physicalQueueService
            );

            this.queueShardMonitor.Start().Wait();
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the current maximum storage capacity of the virtual queue.
        /// </summary>
        public long MaxQueueSizeMegabytes
        {
            get
            {
                return this.queueShardMonitor.Shards.Sum(s => s.MaxSizeMegabytes);
            }
        }
    }
}
