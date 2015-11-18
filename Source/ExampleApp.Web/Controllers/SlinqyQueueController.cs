namespace ExampleApp.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.ServiceBus;
    using Models;
    using Slinqy.Core;

    /// <summary>
    /// Provides APIs for interacting with the SlinqyQueue.
    /// </summary>
    public class SlinqyQueueController : ApiController
    {
        /// <summary>
        /// Used to manage Service Bus resources.
        /// </summary>
        private static readonly NamespaceManager ServiceBusNamespaceManager = NamespaceManager.CreateFromConnectionString(
            ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]
        );

        /// <summary>
        /// Used to interact with virtual queues.
        /// </summary>
        private static readonly SlinqyQueueClient SlinqyQueueClient = new SlinqyQueueClient(
            createPhysicalQueueDelegate: CreateServiceBusQueue,
            listPhysicalQueuesDelegate: ListServiceBusQueues
        );

        /// <summary>
        /// Handles the HTTP POST /Home/CreateQueue by creating a queue with the specified parameters.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue to get.</param>
        /// <returns>Returns the result of the action.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "WebAPI will not route static methods.")]
        [HttpGet]
        [Route("api/slinqy-queue", Name = "GetQueue")]
        public
        QueueInformationModel
        GetQueue(
            string queueName)
        {
            var queue = SlinqyQueueClient.Get(queueName);

            return new QueueInformationModel(
                queue.Name,
                queue.MaxQueueSizeMegabytes
            );
        }

        /// <summary>
        /// Handles the HTTP POST /Home/CreateQueue by creating a queue with the specified parameters.
        /// </summary>
        /// <param name="createQueueModel">Specifies the parameters of the queue to create.</param>
        /// <returns>Returns the result of the action.</returns>
        [HttpPost]
        [Route("api/slinqy-queue", Name = "CreateQueue")]
        public
        async Task<QueueInformationModel>
        CreateQueue(
            CreateQueueModel createQueueModel)
        {
            if (createQueueModel == null)
                throw new ArgumentNullException(nameof(createQueueModel));

            var queue = await SlinqyQueueClient.CreateAsync(createQueueModel.QueueName);

            return new QueueInformationModel(
                queue.Name,
                createQueueModel.MaxQueueSizeMegabytes
            );
        }

        /// <summary>
        /// Creates a new physical queue with the specified name.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue to create.</param>
        /// <returns>Returns a SlinqyQueueShard instance that represents the newly created queue shard.</returns>
        private
        static
        async Task<SlinqyQueueShard>
        CreateServiceBusQueue(
            string queueName)
        {
            var queueDescription = await ServiceBusNamespaceManager.CreateQueueAsync(queueName);

            var slinqyQueueShard = new SlinqyQueueShard(
                queueDescription.Path,
                queueDescription.MaxSizeInMegabytes
            );

            return slinqyQueueShard;
        }

        /// <summary>
        /// Lists all the physical queues starting with the specified name.
        /// </summary>
        /// <param name="queueNamePrefix">
        /// Specifies the prefix to search for.
        /// </param>
        /// <returns>Returns a collection of match queues.</returns>
        private
        static
        async Task<IEnumerable<SlinqyQueueShard>>
        ListServiceBusQueues(
            string queueNamePrefix)
        {
            var serviceBusQueues = (await ServiceBusNamespaceManager.GetQueuesAsync("startswith(path, '" + queueNamePrefix + "-') eq true").ConfigureAwait(false))
                .ToArray();

            var slinqyQueueShards = serviceBusQueues.Select(q =>
                new SlinqyQueueShard(
                    q.Path,
                    q.MaxSizeInMegabytes
                )
            );

            return slinqyQueueShards;
        }
    }
}