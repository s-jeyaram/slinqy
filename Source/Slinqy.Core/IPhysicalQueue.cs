namespace Slinqy.Core
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a physical queue under a queuing service (IPhysicalQueueService).
    /// This interface defines methods for interacting with the queue.
    /// This is the interface that users of the Slinqy library implement so that Slinqy can use their particular queuing service.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Queue is not a suffix in this case.")]
    public interface IPhysicalQueue
    {
        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the maximum capacity for this physical queue shard.
        /// </summary>
        long MaxSizeMegabytes { get; }

        /// <summary>
        /// Gets the current amount of storage, in bytes, that is being used by queued messages.
        /// </summary>
        long CurrentSizeBytes { get; }

        /// <summary>
        /// Gets a boolean value that indicates if the queue is writable (true) or not (false).
        /// </summary>
        bool Writable { get; }

        /// <summary>
        /// Sends a batch of messages to the queue.
        /// </summary>
        /// <param name="batch">Specifies the batch to send.</param>
        /// <returns>Returns an async Task of the work.</returns>
        Task SendBatch(IEnumerable<object> batch);
    }
}
