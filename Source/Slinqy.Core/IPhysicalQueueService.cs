namespace Slinqy.Core
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a queuing service, such as Azure Service Bus, RabbitMQ, etc...
    /// This interface defines methods that act against the queuing service.
    /// This is the interface that users of the Slinqy library implement so that Slinqy can use their particular queuing service.
    /// </summary>
    public interface IPhysicalQueueService
    {
        /// <summary>
        /// Creates a physical queue in the service with the specified name
        /// that is enabled for both sending and receiving of queue messages.
        /// </summary>
        /// <param name="name">
        /// Specifies the name of the queue to create.
        /// </param>
        /// <returns>Returns the queue that was created.</returns>
        Task<IPhysicalQueue> CreateQueue(string name);

        /// <summary>
        /// Creates a physical queue in the service with the specified name
        /// that is only enabled for sending of queue messages to the queue.
        /// </summary>
        /// <param name="name">The name of the queue to create.</param>
        /// <returns>Returns the queue that was created.</returns>
        Task<IPhysicalQueue> CreateSendOnlyQueue(string name);

        /// <summary>
        /// Lists the physical queues whose names matches the specified prefix.
        /// </summary>
        /// <param name="namePrefix">
        /// Specifies the name prefix to list.
        /// </param>
        /// <returns>
        /// Returns a list of matching queues.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule wasn't designed for async Tasks.")]
        Task<IEnumerable<IPhysicalQueue>> ListQueues(string namePrefix);

        /// <summary>
        /// Modifies the status of the specified queue to where clients can only receive from the queue, sending is disabled.
        /// </summary>
        /// <param name="name">Specifies the name of the queue to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetQueueReceiveOnly(string name);
    }
}
