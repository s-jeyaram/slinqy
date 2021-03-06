﻿namespace ExampleApp.Web.Controllers
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Ajax.Utilities;
    using Models;
    using Slinqy.Core;

    /// <summary>
    /// Provides APIs for interacting with the SlinqyQueue.
    /// </summary>
    public class SlinqyQueueController : ApiController
    {
        // The following static fields are making up for not having proper dependency injection in place.
        // Use DI if dependencies become more than a handful of fields...

        // TODO: Create known queues on app-start to better mimic how a real application would operate
        //       and so the static dependencies below can also be handled in a more standard fashion.

        /// <summary>
        /// The queue service for managing queue resources.
        /// </summary>
        private static readonly IPhysicalQueueService PhysicalQueueService = new ServiceBusQueueServiceModel(
            ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]
        );

        /// <summary>
        /// Maintains a list of references to SlinqyQueue's that have been instantiated since they are expensive to create.
        /// </summary>
        private static readonly Dictionary<string, SlinqyQueue> SlinqyQueues = new Dictionary<string, SlinqyQueue>();

        /// <summary>
        /// Maintains a list of queue fill operations.
        /// </summary>
        private static readonly ConcurrentDictionary<string, FillQueueStatusViewModel> FillOperations = new ConcurrentDictionary<string, FillQueueStatusViewModel>();

        /// <summary>
        /// Maintains a list of queue receive operations.
        /// </summary>
        private static readonly ConcurrentDictionary<string, ReceiveQueueStatusViewModel> ReceiveOperations = new ConcurrentDictionary<string, ReceiveQueueStatusViewModel>();

        /// <summary>
        /// Tracks the last created queue.
        /// This currently does not support running multiple instances of the website.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Storing statically until proper DI is available.")]
        private static SlinqyAgent slinqyAgent;

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
            var queue = SlinqyQueues[queueName];

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
        async Task<HttpResponseMessage>
        CreateQueue(
            CreateQueueCommandModel createQueueModel)
        {
            if (createQueueModel == null)
                throw new ArgumentNullException(nameof(createQueueModel));

            if (!this.ModelState.IsValid)
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, this.ModelState);

            var monitor = new SlinqyQueueShardMonitor(
                createQueueModel.QueueName,
                PhysicalQueueService
            );

            slinqyAgent = new SlinqyAgent(
                PhysicalQueueService,
                monitor,
                createQueueModel.StorageCapacityScaleOutThresholdPercentage / 100D,
                4
            );

            await slinqyAgent.Start();

            SlinqyQueues.Add(
                createQueueModel.QueueName,
                new SlinqyQueue(monitor)
            );

            var response = new HttpResponseMessage(HttpStatusCode.Created) {
                Content = new ObjectContent<QueueInformationViewModel>(
                    new QueueInformationViewModel(
                        queueName:              createQueueModel.QueueName,
                        maxQueueSizeMegabytes:  createQueueModel.MaxQueueSizeMegabytes,
                        currentQueueSizeBytes:  0
                    ),
                    new JsonMediaTypeFormatter()
                )
            };

            return response;
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
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "WebAPI will not route static methods.")]
        public
        FillQueueStatusViewModel
        GetFillQueueStatus(
            string queueName)
        {
            return FillOperations[queueName];
        }

        /// <summary>
        /// Handles the HTTP POST /api/slinqy-queue/{queueName}/receive-request by receiving all the messages in the specified queue.
        /// </summary>
        /// <param name="queueName">
        /// Specifies the name of the Slinqy queue to receive.
        /// </param>
        [HttpPost]
        [Route("api/slinqy-queue/{queueName}/receive-request", Name = "ReceiveQueue")]
        public
        void
        StartReceiveQueue(
            string queueName)
        {
            ReceiveOperations.AddOrUpdate(
                key:                queueName,
                addValueFactory:    name => new ReceiveQueueStatusViewModel { Status = ReceiveQueueStatus.Running },
                updateValueFactory: (name, receiveOperation) => {
                    if (receiveOperation.Status != ReceiveQueueStatus.Finished)
                        throw new InvalidOperationException("A previous receive operation is still in progress.");

                    return new ReceiveQueueStatusViewModel { Status = ReceiveQueueStatus.Running };
                }

            );

            // Start the async task.
            this.ReceiveQueue(queueName)
                .ConfigureAwait(false);

            // Return while the task continues to run in the background.
        }

        /// <summary>
        /// Handles the HTTP GET /api/slinqy-queue/{queueName}/receive-request by returning the current status.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue to get the receive request status for.</param>
        /// <returns>Returns information about the receive status.</returns>
        [HttpGet]
        [Route("api/slinqy-queue/{queueName}/receive-request", Name = "GetReceiveQueueStatus")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "WebAPI will not route static methods.")]
        public
        ReceiveQueueStatusViewModel
        GetReceiveQueueStatus(
            string queueName)
        {
            return ReceiveOperations[queueName];
        }

        /// <summary>
        /// Handles the HTTP POST /api/slinqy-queue/{queueName}/messages by submitting specified messages.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue.</param>
        /// <param name="sendMessageCommand">Specifies the parameters for sending a message.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpPost]
        [Route("api/slinqy-queue/{queueName}/messages", Name = "SendMessage")]
        public
        async Task
        SendMessage(
            string                  queueName,
            SendMessageCommandModel sendMessageCommand)
        {
            if (sendMessageCommand == null)
                throw new ArgumentNullException(nameof(sendMessageCommand));

            var queue = SlinqyQueues[queueName];

            await queue
                .Send(sendMessageCommand.MessageBody)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Handles the HTTP POST /api/slinqy-queue/{queueName}/messages/next by returning the next message from the queue.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet]
        [Route("api/slinqy-queue/{queueName}/messages", Name = "ReceiveMessage")]
        public
        async Task<string>
        ReceiveMessage(
            string queueName)
        {
            var queue = SlinqyQueues[queueName];

            return await queue
                .Receive<string>()
                .ConfigureAwait(false);
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
            var queue = SlinqyQueues[queueName];

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
        /// Attempts to receive the specified Slinqy queue until it's empty.
        /// </summary>
        /// <param name="queueName">
        /// Specifies the name of the Slinqy queue to receive.
        /// </param>
        /// <returns>Returns an async Task for the work.</returns>
        private
        async Task
        ReceiveQueue(
            string queueName)
        {
            // Get the queue.
            var queue = SlinqyQueues[queueName];

            while (queue.CurrentQueueSizeBytes > 0)
            {
                try
                {
                    // Receive the batch of messages.
                    var maxWaitTimeSpan = TimeSpan.FromSeconds(30);
                    var receivedBatch   = await queue.ReceiveBatch<byte[]>(maxWaitTimeSpan)
                        .ConfigureAwait(false);

                    ReceiveOperations[queueName]
                        .ReceivedCount += receivedBatch.Count();
                }
                catch (Exception exception)
                {
                    Trace.TraceWarning(
                        "Exception while receiving batch (will retry):\r\n" + exception
                    );
                }
            }

            ReceiveOperations[queueName].Status = ReceiveQueueStatus.Finished;
        }
    }
}