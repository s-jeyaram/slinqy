namespace ExampleApp.Web.Controllers
{
    using System;
    using Microsoft.ServiceBus;
    using Models;
    using System.Configuration;
    using System.Web.Mvc;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// Defines supported actions for the Homepage.
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Used to manage Service Bus resources.
        /// </summary>
        private readonly NamespaceManager serviceBusNamespaceManager;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public 
        HomeController()
        {
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
        ActionResult 
        CreateQueue(
            CreateQueueModel createQueueModel)
        {
            if (createQueueModel == null)
                throw new ArgumentNullException("createQueueModel");

            var queueDescription = new QueueDescription(createQueueModel.QueueName)
            {
                MaxSizeInMegabytes = createQueueModel.MaxQueueSizeMegabytes
            };
            
            queueDescription = this.serviceBusNamespaceManager.CreateQueue(
                queueDescription
            );

            return this.PartialView(
                "ManageQueue", 
                new ManageQueueModel(
                    createQueueModel.QueueName, 
                    queueDescription.MaxSizeInMegabytes)
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
    }
}
