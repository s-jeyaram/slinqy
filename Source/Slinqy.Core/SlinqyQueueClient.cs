namespace Slinqy.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading.Tasks;

    /// <summary>
    /// Wraps calls to your queue client of choice.
    /// </summary>
    public class SlinqyQueueClient
    {
        /// <summary>
        /// Delegate to the function for creating new physical queues.
        /// </summary>
        private readonly Func<string, Task<SlinqyQueueShard>> createPhysicalQueueDelegate;

        /// <summary>
        /// Delegate to the function for listing physical queues.
        /// </summary>
        private readonly Func<string, Task<IEnumerable<SlinqyQueueShard>>> listPhysicalQueuesDelegate;

        /// <summary>
        /// Maintains a list of references to SlinqyQueue's that have been instantiated since they are expenstive to create.
        /// </summary>
        private readonly ConcurrentDictionary<string, SlinqyQueue> slinqyQueues = new ConcurrentDictionary<string, SlinqyQueue>();

            /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueClient"/> class.
        /// </summary>
        /// <param name="createPhysicalQueueDelegate">
        /// Specifies the function to use for creating new physical queue shards.
        /// </param>
        /// <param name="listPhysicalQueuesDelegate">
        /// Specifies the function to use for listing all the physical queues whose names start with the specified string.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if any of the specified delegate parameters are not specified.</exception>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was not designed for async calls.")]
        public
        SlinqyQueueClient(
            Func<string, Task<SlinqyQueueShard>>                createPhysicalQueueDelegate,
            Func<string, Task<IEnumerable<SlinqyQueueShard>>>   listPhysicalQueuesDelegate)
        {
            if (createPhysicalQueueDelegate == null)
                throw new ArgumentNullException(nameof(createPhysicalQueueDelegate));
            if (listPhysicalQueuesDelegate == null)
                throw new ArgumentNullException(nameof(listPhysicalQueuesDelegate));

            this.createPhysicalQueueDelegate = createPhysicalQueueDelegate;
            this.listPhysicalQueuesDelegate  = listPhysicalQueuesDelegate;
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
            var shard = await this.createPhysicalQueueDelegate(queueShardName);

            var queue = new SlinqyQueue(
                queueName,
                this.listPhysicalQueuesDelegate
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
                name => new SlinqyQueue(queueName, this.listPhysicalQueuesDelegate)
            );

            return queue;
        }
    }
}
