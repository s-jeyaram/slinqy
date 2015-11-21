namespace ExampleApp.Web.Controllers
{
    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Models;
    using Slinqy.Core;

    /// <summary>
    /// Provides APIs for interacting with the SlinqyQueue.
    /// </summary>
    public class SlinqyQueueController : ApiController
    {
        // The following static fields are making up for not having proper dependency injection in place.
        // Use DI if dependencies become more than a handful of fields...

        /// <summary>
        /// The queue service for managing queue resources.
        /// </summary>
        private static readonly IPhysicalQueueService PhysicalQueueService = new ServiceBusQueueService(
            ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]
        );

        /// <summary>
        /// Used to interact with virtual queues.
        /// </summary>
        private static readonly SlinqyQueueClient SlinqyQueueClient = new SlinqyQueueClient(
            PhysicalQueueService
        );

        /// <summary>
        /// Tracks the last created queue.
        /// This currently does not support running multiple instances of the website.
        /// </summary>
        // TODO: Modify to support running on multiple instances.
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Storing statically until proper DI is available.")]
        private static SlinqyAgent slinqyAgent;

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

            slinqyAgent = new SlinqyAgent(queue.Name, PhysicalQueueService, 0.25);

            return new QueueInformationModel(
                queue.Name,
                createQueueModel.MaxQueueSizeMegabytes
            );
        }
    }
}