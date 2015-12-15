namespace ExampleApp.Web.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Slinqy.Core;

    /// <summary>
    /// Implements the IPhysicalQueueService interface for the Azure Service Bus service.
    /// </summary>
    public class ServiceBusQueueServiceModel : IPhysicalQueueService
    {
        /// <summary>
        /// The connection string used to interact with the Service Bus.
        /// </summary>
        private readonly string serviceBusConnectionString;

        /// <summary>
        /// Used to manage Service Bus resources.
        /// </summary>
        private readonly NamespaceManager serviceBusNamespaceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusQueueServiceModel"/> class.
        /// </summary>
        /// <param name="serviceBusConnectionString">Specifies the connection string for the Azure Service Bus.</param>
        public
        ServiceBusQueueServiceModel(
            string serviceBusConnectionString)
        {
            this.serviceBusConnectionString = serviceBusConnectionString;

            this.serviceBusNamespaceManager = NamespaceManager.CreateFromConnectionString(
                this.serviceBusConnectionString
            );
        }

        /// <summary>
        /// Creates the specified physical Service Bus queue.
        /// </summary>
        /// <param name="name">Specifies the queue path.</param>
        /// <returns>Returns the created queue as a IPhysicalQueue.</returns>
        public
        async Task<IPhysicalQueue>
        CreateQueue(
            string name)
        {
            return await this.CreateQueueInternal(new QueueDescription(name));
        }

        /// <summary>
        /// Creates a new Service Bus queue in the ReceiveDisabled state.
        /// </summary>
        /// <param name="name">Specifies the queue path.</param>
        /// <returns>Returns the created queue as an IPhysicalQueue.</returns>
        public
        async Task<IPhysicalQueue>
        CreateSendOnlyQueue(
            string name)
        {
            var queueDescription = new QueueDescription(name) {
                Status = EntityStatus.ReceiveDisabled
            };

            return await this.CreateQueueInternal(queueDescription);
        }

        /// <summary>
        /// Gets a list of queues matching the specified path prefix.
        /// </summary>
        /// <param name="namePrefix">Specifies the prefix to search for.</param>
        /// <returns>Returns the matching shards.</returns>
        public
        async Task<IEnumerable<IPhysicalQueue>>
        ListQueues(
            string namePrefix)
        {
            var getFilter = "startswith(path, '" + namePrefix + "') eq true";

            var queues = (await this.serviceBusNamespaceManager.GetQueuesAsync(getFilter)
                .ConfigureAwait(false))
                .Select(sbq => new ServiceBusQueueModel(this.serviceBusConnectionString, sbq))
                .ToArray();

            return queues;
        }

        /// <summary>
        /// Sets the specified Service Bus queue in to a Receive-Only mode.
        /// </summary>
        /// <param name="name">Specifies the queue path.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public
        async Task
        SetQueueReceiveOnly(
            string name)
        {
            var queue = await this.serviceBusNamespaceManager.GetQueueAsync(name);

            queue.Status = EntityStatus.SendDisabled;

            await this.serviceBusNamespaceManager.UpdateQueueAsync(queue);
        }

        /// <summary>
        /// Sets the specified Service Bus queue in to a Disabled mode.
        /// </summary>
        /// <param name="name">Specifies the queue path.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public
        async Task
        SetQueueDisabled(
            string name)
        {
            var queue = await this.serviceBusNamespaceManager.GetQueueAsync(name);

            queue.Status = EntityStatus.Disabled;

            await this.serviceBusNamespaceManager.UpdateQueueAsync(queue);
        }

        /// <summary>
        /// Sets the specified Service Bus queue in to a Enabled mode for both reading and writing.
        /// </summary>
        /// <param name="name">Specifies the queue path.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public
        async Task
        SetQueueEnabled(
            string name)
        {
            var queue = await this.serviceBusNamespaceManager.GetQueueAsync(name);

            queue.Status = EntityStatus.Active;

            await this.serviceBusNamespaceManager.UpdateQueueAsync(queue);
        }

        /// <summary>
        /// Creates the specified physical Service Bus queue using the specified description.
        /// </summary>
        /// <param name="queueDescription">Specifies the details of the queue to create.</param>
        /// <returns>Returns the created queue as a IPhysicalQueue.</returns>
        private
        async Task<IPhysicalQueue>
        CreateQueueInternal(
            QueueDescription queueDescription)
        {
            await this.serviceBusNamespaceManager.CreateQueueAsync(queueDescription)
                .ConfigureAwait(false);

            var physicalQueue = new ServiceBusQueueModel(
                this.serviceBusConnectionString,
                queueDescription
            );

            return physicalQueue;
        }
    }
}