namespace Slinqy.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Threading.Tasks;

    /// <summary>
    /// The main class responsible for monitoring and managing queue resources.
    /// </summary>
    public class SlinqyAgent
    {
        /// <summary>
        /// Maintains a reference to the function that can be used to create a new physical queues.
        /// </summary>
        private readonly Func<string, Task<SlinqyQueueShard>> createPhysicalQueueDelegate;

        /// <summary>
        /// The reference to the function that returns a current list of physical queues matching the specified prefix.
        /// </summary>
        private readonly Func<string, Task<IEnumerable<SlinqyQueueShard>>> listPhysicalQueuesDelegate;

        /// <summary>
        /// The treshold at which the agent should scale out storage capacity.
        /// </summary>
        private double storageCapacityScaleOutThreshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyAgent"/> class.
        /// </summary>
        /// <param name="createPhysicalQueueDelegate">
        ///     Specifies the function to call to create a new physical queue.
        /// </param>
        /// <param name="listPhysicalQueuesDelegate">
        ///     Specifies the function to call to get a list of physical queues matching the specified prefix.
        /// </param>
        /// <param name="storageCapacityScaleOutThreshold">
        /// Specifies at what percentage of the current physical write queues storage
        /// utilization that the agent should take a scale out action (add another shard).
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was not designed for async calls.")]
        public
        SlinqyAgent(
            Func<string, Task<SlinqyQueueShard>>                createPhysicalQueueDelegate,
            Func<string, Task<IEnumerable<SlinqyQueueShard>>>   listPhysicalQueuesDelegate,
            double                                              storageCapacityScaleOutThreshold)
        {
            this.createPhysicalQueueDelegate        = createPhysicalQueueDelegate;
            this.listPhysicalQueuesDelegate         = listPhysicalQueuesDelegate;
            this.storageCapacityScaleOutThreshold   = storageCapacityScaleOutThreshold;
        }

        /// <summary>
        /// Starts the agent to begin monitoring the queue shards and taking action if needed.
        /// </summary>
        public
        void
        Start()
        {
            this.storageCapacityScaleOutThreshold.ToString(CultureInfo.InvariantCulture);
            this.listPhysicalQueuesDelegate("fdsa").Wait();
            this.createPhysicalQueueDelegate("shard-1").Wait();
        }
    }
}
