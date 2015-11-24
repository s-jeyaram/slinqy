namespace ExampleApp.Web.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
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
        /// <returns>Returns the created queue as a SlinqyQueueShard.</returns>
        public
        async Task<IPhysicalQueue>
        CreateQueue(
            string name)
        {
            var queueDescription = await this.serviceBusNamespaceManager.CreateQueueAsync(path: name);

            var physicalQueue = new ServiceBusQueueModel(
                this.serviceBusConnectionString,
                queueDescription
            );

            return physicalQueue;
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
            var getFilter = "startswith(path, '" + namePrefix + "-') eq true";

            var queues = (await this.serviceBusNamespaceManager.GetQueuesAsync(getFilter)
                .ConfigureAwait(false))
                .Select(sbq => new ServiceBusQueueModel(this.serviceBusConnectionString, sbq))
                .ToArray();

            return queues;
        }
    }
}