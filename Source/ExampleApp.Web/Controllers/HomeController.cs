namespace ExampleApp.Web.Controllers
{
    using Microsoft.ServiceBus;
    using Models;
    using Slinqy.Core;
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    /// <summary>
    /// Defines supported actions for the Homepage.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Used to manage Service Bus resources.
        /// </summary>
        private readonly NamespaceManager   serviceBusNamespaceManager;

        /// <summary>
        /// Used to interact with physical queues.
        /// </summary>
        private readonly SlinqyQueueClient  slinqyQueueClient;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public 
        HomeController()
        {
            this.slinqyQueueClient = new SlinqyQueueClient(
                createPhysicalQueueDelegate: this.CreateServiceBusQueue
            );

            this.serviceBusNamespaceManager = NamespaceManager.CreateFromConnectionString(
                ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]
            );
        }

        /// <summary>
        /// Handles the default HTTP GET /.
        /// </summary>
        /// <returns>Returns the default Homepage view.</returns>
        public 
        ActionResult 
        Index()
        {
            this.ViewBag.Title = "Home Page";

            return this.View();
        }

        /// <summary>
        /// Creates a queue with the specified parameters.
        /// </summary>
        /// <param name="createQueueModel">Specifies the parameters of the queue to create.</param>
        /// <returns>Returns the result of the action.</returns>
        public 
        async Task<ActionResult>
        CreateQueue(
            CreateQueueModel createQueueModel)
        {
            if (createQueueModel == null)
                throw new ArgumentNullException("createQueueModel");

            var queue = await this.slinqyQueueClient.CreateAsync(createQueueModel.QueueName);

            return this.PartialView(
                "ManageQueue", 
                new ManageQueueModel(
                    queue.Name, 
                    queue.MaximumSizeInMegabytes
                )
            );
        }

        /// <summary>
        /// Applies the specified settings to the queue.
        /// </summary>
        /// <param name="manageQueueModel">Specifies new settings for the queue.</param>
        public 
        void 
        ManageQueue(
            ManageQueueModel manageQueueModel)
        {
            this.ToString();

            if (manageQueueModel != null)
                manageQueueModel.ToString();
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue to create.</param>
        /// <returns>Returns a SlinqyQueue instance that represents the virtual queue.</returns>
        private
        async Task<SlinqyQueue>
        CreateServiceBusQueue(
            string queueName)
        {
            var queueDescription = await this.serviceBusNamespaceManager.CreateQueueAsync(queueName);

            var slinqyQueue = new SlinqyQueue(
                queueDescription.Path,
                queueDescription.MaxSizeInMegabytes
            );

            return slinqyQueue;
        }
    }
}
