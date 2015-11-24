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
            this.serviceBusNamespaceManager = NamespaceManager.CreateFromConnectionString(
                serviceBusConnectionString
            );
        }

        /// <summary>
        /// Creates the specified physical Service Bus queue.
        /// </summary>
        /// <param name="name">Specifies the queue path.</param>
        /// <returns>Returns the created queue as a SlinqyQueueShard.</returns>
        public
        async Task<SlinqyQueueShard>
        CreateQueue(
            string name)
        {
            var queueDescription = await this.serviceBusNamespaceManager.CreateQueueAsync(name);

            var slinqyQueueShard = new SlinqyQueueShard(
                queueDescription.Path,
                0,
                queueDescription.MaxSizeInMegabytes,
                queueDescription.SizeInBytes * 1024,
                true
            );

            return slinqyQueueShard;
        }

        /// <summary>
        /// Gets a list of queues matching the specified path prefix.
        /// </summary>
        /// <param name="namePrefix">Specifies the prefix to search for.</param>
        /// <returns>Returns the matching shards.</returns>
        public
        async Task<IEnumerable<SlinqyQueueShard>>
        ListQueues(
            string namePrefix)
        {
            var serviceBusQueues = (await this.serviceBusNamespaceManager.GetQueuesAsync("startswith(path, '" + namePrefix + "-') eq true").ConfigureAwait(false))
                .ToArray();

            var slinqyQueueShards = serviceBusQueues.Select(q =>
                new SlinqyQueueShard(
                    q.Path,
                    0, // TODO: Maybe the SlinqyQueue class should be responsible for instantiating and providing it's path + index.
                    q.MaxSizeInMegabytes,
                    q.SizeInBytes * 1024,
                    q.Status == EntityStatus.Active || q.Status == EntityStatus.ReceiveDisabled
                )
            );

            return slinqyQueueShards;
        }
    }
}