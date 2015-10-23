namespace Slinqy.Core
{
    using System;
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
        private readonly Func<string, Task<SlinqyQueue>> createPhysicalQueueDelegate;

        /// <summary>
        /// Initializes a new queue client instance.
        /// </summary>
        /// <param name="createPhysicalQueueDelegate">
        /// Specifies the function to use for creating new physical queue shards.
        /// </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was not designed for async calls.")]
        public
        SlinqyQueueClient(
            Func<string, Task<SlinqyQueue>> createPhysicalQueueDelegate)
        {
            if (createPhysicalQueueDelegate == null)
                throw new ArgumentNullException("createPhysicalQueueDelegate");

            this.createPhysicalQueueDelegate = createPhysicalQueueDelegate;
        }

        /// <summary>
        /// Creates a new queue.
        /// </summary>
        /// <param name="queueName">
        /// Specifies the name of the queue.
        /// </param>
        /// <returns>Returns the resulting SlinqyQueue that was created.</returns>
        public 
        Task<SlinqyQueue>
        CreateAsync(
            string queueName)
        {
            var queueShardName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}-{1}",
                queueName,
                0
            );
            
            // Call the function to create the physical queue shard.
            return this.createPhysicalQueueDelegate(queueShardName);
        }
    }
}
