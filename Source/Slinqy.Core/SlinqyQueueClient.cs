namespace Slinqy.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading.Tasks;

    /// <summary>
    /// Wraps calls to your queue client of choice.
    /// </summary>
    public class SlinqyQueueClient
    {
        /// <summary>
        /// Maintains a list of references to SlinqyQueue's that have been instantiated since they are expenstive to create.
        /// </summary>
        private readonly ConcurrentDictionary<string, SlinqyQueue> slinqyQueues = new ConcurrentDictionary<string, SlinqyQueue>();

        /// <summary>
        /// The physical queue service to use for managing queue resources.
        /// </summary>
        private IPhysicalQueueService physicalQueueService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueClient"/> class.
        /// </summary>
        /// <param name="queueService">
        /// Specifies the IPhysicalQueueService to use for managing queue resources.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if any of the specified parameters are not specified.</exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was not designed for async calls.")]
        public
        SlinqyQueueClient(
            IPhysicalQueueService queueService)
        {
            if (queueService == null)
                throw new ArgumentNullException(nameof(queueService));

            this.physicalQueueService = queueService;
        }

        /// <summary>
        /// Creates a new queue.
        /// </summary>
        /// <param name="queueName">
        /// Specifies the name of the queue.
        /// </param>
        /// <returns>Returns the resulting SlinqyQueue that was created.</returns>
        public
        async Task<SlinqyQueue>
        CreateAsync(
            string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName));

            var queueShardName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}-{1}",
                queueName,
                0
            );

            // Call the function to create the first physical queue shard to establish the virtual queue.
            // No need to do anything with the returned shard...
            var shard = await this.physicalQueueService.CreateQueue(queueShardName);

            var queue = new SlinqyQueue(
                queueName,
                this.physicalQueueService
            );

            this.slinqyQueues.TryAdd(queueName, queue);

            return queue;
        }

        /// <summary>
        /// Gets the specified Slinqy queue.
        /// </summary>
        /// <param name="queueName">Specifies the name of the virtual queue to get.</param>
        /// <returns>Returns a SlinqyQueue instance that can be used to interact with the Slinqy queue.</returns>
        public
        SlinqyQueue
        Get(
            string queueName)
        {
            var queue = this.slinqyQueues.GetOrAdd(
                queueName,
                name => new SlinqyQueue(queueName, this.physicalQueueService)
            );

            return queue;
        }
    }
}
