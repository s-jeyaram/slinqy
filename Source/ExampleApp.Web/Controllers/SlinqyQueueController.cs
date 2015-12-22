namespace ExampleApp.Web.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Configuration;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
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
        private static readonly IPhysicalQueueService PhysicalQueueService = new ServiceBusQueueServiceModel(
            ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]
        );

        /// <summary>
        /// Used to interact with virtual queues.
        /// </summary>
        private static readonly SlinqyQueueClient SlinqyQueueClient = new SlinqyQueueClient(
            PhysicalQueueService
        );

        /// <summary>
        /// Maintains a list of queue fill operations.
        /// </summary>
        private static readonly ConcurrentDictionary<string, FillQueueStatusViewModel> FillOperations = new ConcurrentDictionary<string, FillQueueStatusViewModel>();

        /// <summary>
        /// Maintains a list of queue read operations.
        /// </summary>
        private static readonly ConcurrentDictionary<string, ReadQueueStatusViewModel> ReadOperations = new ConcurrentDictionary<string, ReadQueueStatusViewModel>();

        /// <summary>
        /// Tracks the last created queue.
        /// This currently does not support running multiple instances of the website.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Storing statically until proper DI is available.")]
        private static SlinqyAgent slinqyAgent; // TODO: Modify to support running on multiple instances.

        /// <summary>
        /// Handles HTTP GET /api/slinqy-queue/{queueName} by returning information about the requested queue.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue to get.</param>
        /// <returns>Returns the result of the action.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "WebAPI will not route static methods.")]
        [HttpGet]
        [Route("api/slinqy-queue/{queueName}", Name = "GetQueue")]
        public
        QueueInformationViewModel
        GetQueue(
            string queueName)
        {
            var queue = SlinqyQueueClient.Get(queueName);

            return new QueueInformationViewModel(
                queue.Name,
                queue.MaxQueueSizeMegabytes,
                queue.CurrentQueueSizeBytes
            );
        }

        /// <summary>
        /// Handles the HTTP POST /api/slinqy-queue by creating a queue with the specified parameters.
        /// </summary>
        /// <param name="createQueueModel">Specifies the parameters of the queue to create.</param>
        /// <returns>Returns the result of the action.</returns>
        [HttpPost]
        [Route("api/slinqy-queue", Name = "CreateQueue")]
        public
        async Task<QueueInformationViewModel>
        CreateQueue(
            CreateQueueCommandModel createQueueModel)
        {
            if (createQueueModel == null)
                throw new ArgumentNullException(nameof(createQueueModel));

            var queue = await SlinqyQueueClient.CreateQueueAsync(createQueueModel.QueueName);

            var monitor = new SlinqyQueueShardMonitor(
                createQueueModel.QueueName,
                PhysicalQueueService
            );

            slinqyAgent = new SlinqyAgent(
                PhysicalQueueService,
                monitor,
                createQueueModel.StorageCapacityScaleOutThresholdPercentage / 100D
            );

            slinqyAgent.Start();

            return new QueueInformationViewModel(
                queue.Name,
                queue.MaxQueueSizeMegabytes,
                queue.CurrentQueueSizeBytes
            );
        }

        /// <summary>
        /// Handles the HTTP POST /api/slinqy-queue/{queueName}/fill-request by submitting randomly generated messages.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue.</param>
        /// <param name="fillQueueCommand">Specifies the amount of data, in megabytes, to submit to the queue.</param>
        [HttpPost]
        [Route("api/slinqy-queue/{queueName}/fill-request", Name = "FillQueue")]
        public
        void
        StartFillQueue(
            string                queueName,
            FillQueueCommandModel fillQueueCommand)
        {
            if (fillQueueCommand == null)
                throw new ArgumentNullException(nameof(fillQueueCommand));

            // Start the async task.
            this.FillQueue(queueName, fillQueueCommand.SizeMegabytes)
                .ConfigureAwait(false);

            FillOperations.AddOrUpdate(
                key:                queueName,
                addValueFactory:    name => new FillQueueStatusViewModel { Status = FillQueueStatus.Running },
                updateValueFactory: (name, fillOperation) => {
                    if (fillOperation.Status != FillQueueStatus.Finished)
                        throw new InvalidOperationException("A previous fill operation is still in progress.");

                    return new FillQueueStatusViewModel { Status = FillQueueStatus.Running };
                }

            );

            // Return while the task continues to run in the background.
        }

        /// <summary>
        /// Handles the HTTP GET /api/slinqy-queue/{queueName}/fill-request by returning the current status.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue to get the fill request status for.</param>
        /// <returns>Returns information about the fill status.</returns>
        [HttpGet]
        [Route("api/slinqy-queue/{queueName}/fill-request", Name = "GetFillQueueStatus")]
        public
        FillQueueStatusViewModel
        GetFillQueueStatus(
            string queueName)
        {
            this.ToString();
            return FillOperations[queueName];
        }

        /// <summary>
        /// Handles the HTTP POST /api/slinqy-queue/{queueName}/read-request by reading all the messages in the specified queue.
        /// </summary>
        /// <param name="queueName">
        /// Specifies the name of the Slinqy queue to read.
        /// </param>
        [HttpPost]
        [Route("api/slinqy-queue/{queueName}/read-request", Name = "ReadQueue")]
        public
        void
        StartReadQueue(
            string queueName)
        {
            // Start the async task.
            this.ReadQueue(queueName)
                .ConfigureAwait(false);

            ReadOperations.AddOrUpdate(
                key: queueName,
                addValueFactory: name => new ReadQueueStatusViewModel { Status = ReadQueueStatus.Running },
                updateValueFactory: (name, readOperation) => {
                    if (readOperation.Status != ReadQueueStatus.Finished)
                        throw new InvalidOperationException("A previous read operation is still in progress.");

                    return new ReadQueueStatusViewModel { Status = ReadQueueStatus.Running };
                }

            );

            // Return while the task continues to run in the background.
        }

        /// <summary>
        /// Handles the HTTP GET /api/slinqy-queue/{queueName}/read-request by returning the current status.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue to get the read request status for.</param>
        /// <returns>Returns information about the read status.</returns>
        [HttpGet]
        [Route("api/slinqy-queue/{queueName}/read-request", Name = "GetReadQueueStatus")]
        public
        ReadQueueStatusViewModel
        GetReadQueueStatus(
            string queueName)
        {
            this.ToString();
            return ReadOperations[queueName];
        }

        /// <summary>
        /// Attempts to fill the specified Slinqy queue with random data.
        /// </summary>
        /// <param name="queueName">
        /// Specifies the name of the Slinqy queue to fill.
        /// </param>
        /// <param name="sizeMegabytes">Specifies the amount of data, in megabytes, to submit to the queue.</param>
        /// <returns>Returns an async Task for the work.</returns>
        private
        async Task
        FillQueue(
            string  queueName,
            int     sizeMegabytes)
        {
            // Get the queue.
            var queue = SlinqyQueueClient.Get(queueName);

            // Prepare to generate some random data.
            var messagesPerBatch        = 250;
            var randomMessagePayload    = new byte[1024 * 5];
            var ranGen                  = new Random(DateTime.UtcNow.Millisecond);
            var sizeKilobytes           = sizeMegabytes * 1024;
            var kilobytesPerBatch       = messagesPerBatch * (randomMessagePayload.Length / 1024);
            var batches                 = sizeKilobytes / kilobytesPerBatch;

            for (var i = 0; i < batches; i++)
            {
                // Generate random data for each item in the batch.
                var batch = Enumerable.Range(0, messagesPerBatch).Select(index => {
                    // Generate random data.
                    ranGen.NextBytes(randomMessagePayload);

                    return randomMessagePayload;
                }).ToArray();

                try
                {
                    // Send the batch of random data.
                    await queue.SendBatch(batch)
                        .ConfigureAwait(false);

                    FillOperations[queueName]
                        .SentCount += batch.Count();
                }
                catch (Exception exception)
                {
                    Trace.TraceWarning(
                        "Exception while sending batch (will retry):\r\n" + exception
                    );
                }
            }

            FillOperations[queueName].Status = FillQueueStatus.Finished;
        }

        /// <summary>
        /// Attempts to read the specified Slinqy queue until it's empty.
        /// </summary>
        /// <param name="queueName">
        /// Specifies the name of the Slinqy queue to read.
        /// </param>
        /// <returns>Returns an async Task for the work.</returns>
        private
        async Task
        ReadQueue(
            string queueName)
        {
            // Get the queue.
            var queue = SlinqyQueueClient.Get(queueName);

            while (queue.CurrentQueueSizeBytes > 0)
            {
                try
                {
                    // Receive the batch of messages.
                    var maxWaitTimeSpan = TimeSpan.FromSeconds(1);
                    var receivedBatch = await queue.ReceiveBatch(maxWaitTimeSpan)
                        .ConfigureAwait(false);

                    ReadOperations[queueName]
                        .ReceivedCount += receivedBatch.Count();
                }
                catch (Exception exception)
                {
                    Trace.TraceWarning(
                        "Exception while receiving batch (will retry):\r\n" + exception
                    );
                }
            }

            ReadOperations[queueName].Status = ReadQueueStatus.Finished;
        }
    }
}