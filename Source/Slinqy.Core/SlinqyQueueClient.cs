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
        /// Defines the default value for a shard index on a new queue.
        /// </summary>
        private const int DefaultShardIndex = 0;

        /// <summary>
        /// Maintains a list of references to SlinqyQueue's that have been instantiated since they are expensive to create.
        /// </summary>
        private readonly ConcurrentDictionary<string, SlinqyQueue> slinqyQueues = new ConcurrentDictionary<string, SlinqyQueue>();

        /// <summary>
        /// The physical queue service to use for managing queue resources.
        /// </summary>
        private readonly IPhysicalQueueService physicalQueueService;

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
        /// <param name="shardIndexPadding">
        /// Specifies how many digits should be used for the index part of the queue shard name.
        /// For example:
        /// CreateQueueAsync("my-queue", 1) will generate the following: "my-queue0", "my-queue1", "my-queue2", etc.
        /// CreateQueueAsync("my-queue", 2) will generate the following: "my-queue00", "my-queue01", "my-queue02", etc.
        /// </param>
        /// <returns>Returns the resulting SlinqyQueue that was created.</returns>
        public
        async Task<SlinqyQueue>
        CreateQueueAsync(
            string  queueName,
            int     shardIndexPadding)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentNullException(nameof(queueName));

            var queueShardName = SlinqyQueueShard.GenerateFirstShardName(
                slinqyQueueName:    queueName,
                shardIndexPadding:  shardIndexPadding
            );

            // Call the function to create the first physical queue shard to establish the virtual queue.
            // No need to do anything with the returned shard...
            var shard = await this.physicalQueueService
                .CreateQueue(queueShardName)
                .ConfigureAwait(false);

            var shardMonitor = new SlinqyQueueShardMonitor(
                queueName,
                this.physicalQueueService
            );

            await shardMonitor
                .Start()
                .ConfigureAwait(false);

            var queue = new SlinqyQueue(
                shardMonitor
            );

            this.slinqyQueues.TryAdd(queueName, queue);

            return queue;
        }

        /// <summary>
        /// Creates a new queue with a default shardIndexPadding of 1,
        /// which allows for a maximum of 10 (0 - 9).
        /// </summary>
        /// <param name="queueName">
        /// Specifies the name of the queue.
        /// </param>
        /// <returns>Returns the resulting SlinqyQueue that was created.</returns>
        public
        async Task<SlinqyQueue>
        CreateQueueAsync(
            string  queueName)
        {
            return await this.CreateQueueAsync(queueName, 1)
                .ConfigureAwait(false);
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
                name => new SlinqyQueue(new SlinqyQueueShardMonitor(queueName, this.physicalQueueService))
            );

            return queue;
        }
    }
}
