namespace Slinqy.Test.Functional.Steps
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Models;
    using Models.ExampleAppPages;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Defines steps for working with the queues of the Example App.
    /// </summary>
    [Binding]
    public class QueueSteps : BaseSteps
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueSteps"/> class.
        /// </summary>
        /// <param name="browser">
        /// Specifies the WebBrowser instance to use for interacting with the browser.
        /// </param>
        public
        QueueSteps(
            WebBrowser browser)
                : base(browser)
        {
        }

        /// <summary>
        /// Generates and submits queue messages to the queue until storage utilization reaches the configured scale up threshold.
        /// </summary>
        [When]
        public
        static
        void
        WhenTheQueueStorageUtilizationReachesTheScaleUpThreshold()
        {
            ContextGet<ManageQueueSection>()
                .QueueClient
                .FillQueue();
        }

        /// <summary>
        /// Verifies that the queue storage capacity has expanded since the scenario started.
        /// </summary>
        [Then]
        public
        static
        void
        ThenTheQueueStorageCapacityExpands()
        {
            var manageQueueSection = ContextGet<ManageQueueSection>();
            var createQueueParams  = ContextGet<CreateQueueParameters>();

            var pollMaxSeconds     = 30;
            var pollStartTimestamp = DateTimeOffset.UtcNow;

            // TODO: Make poll logic a generic function
            while (DateTimeOffset.UtcNow.Subtract(pollStartTimestamp).TotalSeconds <= pollMaxSeconds)
            {
                if (manageQueueSection.QueueInformation.StorageCapacityMegabytes > createQueueParams.StorageCapacityMegabytes)
                    return;
            }

            Assert.Fail("The Queue Storage Capacity was not increased.");
        }

        /// <summary>
        /// Verifies that all the queue messages can be received.
        /// </summary>
        [Then]
        public
        static
        void
        ThenTheAllTheMessagesCanBeReceived()
        {
            var sentCount = ContextGet<int>("sentCount");
            var receivedCount = ContextGet<int>("receivedCount");

            Assert.AreEqual(sentCount, receivedCount);
        }

        /// <summary>
        /// Starts receiving messages from the queue.
        /// </summary>
        [When]
        public
        static
        void
        WhenTheQueueReceiverIsStarted()
        {
            var receivedCount = QueueSteps.ContextGet<ManageQueueSection>()
                .QueueClient
                .ReceiveQueue();

            QueueSteps.ContextSet(receivedCount, nameof(receivedCount));
        }

        /// <summary>
        /// Sends a randomly generated message to a previously created queue.
        /// </summary>
        [When]
        public
        static
        void
        WhenAMessageIsSent()
        {
            var sentMessage = QueueSteps.ContextGet<ManageQueueSection>()
                .QueueClient
                .SendMessage();

            BaseSteps.ContextSet(sentMessage);
        }

        /// <summary>
        /// Verifies that the message that was originally sent can be received.
        /// </summary>
        [Then]
        public
        static
        void
        ThenTheMessageCanBeReceived()
        {
            var sentMessage = ContextGet<string>();
            var receivedMessage = ContextGet<ManageQueueSection>()
                .QueueClient.ReceiveQueueMessage();

            Assert.AreEqual(
                sentMessage,
                receivedMessage
            );
        }

        /// <summary>
        /// Creates a Queue with the Queue Storage Utilization Scale Up Threshold setting.
        /// </summary>
        [Given]
        public
        void
        GivenAQueueWithStorageUtilizationScaleUpThresholdSet()
        {
            var homepage = this.WebBrowser.NavigateTo<Homepage>();

            // Configure so it doesn't take much to hit the threshold.
            var createQueueParams = new CreateQueueParameters(
                "test-queue",
                storageCapacityMegabytes:   1024,
                scaleUpThresholdPercentage: 1
            );

            var manageQueueSection = homepage
                .CreateQueueForm
                .CreateQueue(createQueueParams);

            QueueSteps.ContextSet(createQueueParams);
            QueueSteps.ContextSet(manageQueueSection);
        }

        /// <summary>
        /// Creates a Queue and fills it until it has scaled out twice, resulting in three shards with data at the end.
        /// </summary>
        [Given]
        public
        void
        GivenAQueueWhoseStorageHasScaledOut()
        {
            var targetNumberOfShards                 = 3;
            var scaleUpThresholdPercentage           = 1;
            var initialQueueStorageCapacityMegabytes = 1024; // 1 GB
            var homepage                             = this.WebBrowser.NavigateTo<Homepage>();

            // Configure so it doesn't take much to hit the threshold.
            var createQueueParams = new CreateQueueParameters(
                queueName:                  "scaled-test-queue",
                storageCapacityMegabytes:   initialQueueStorageCapacityMegabytes,
                scaleUpThresholdPercentage: scaleUpThresholdPercentage
            );

            // Create it!
            QueueSteps.ContextSet(homepage
                .CreateQueueForm
                .CreateQueue(createQueueParams));

            // Calculate amount of data needed to generate to fill 3 shards.
            var megabytesToScale = (int)(initialQueueStorageCapacityMegabytes * (scaleUpThresholdPercentage / 100D)) * targetNumberOfShards;

            // Submit data
            var sentCount = QueueSteps.ContextGet<ManageQueueSection>()
                .QueueClient
                .FillQueue(sizeMegabytes: megabytesToScale);

            QueueSteps.ContextSet(sentCount, nameof(sentCount));
        }

        /// <summary>
        /// Creates a minimal Slinqy Queue with all default settings.
        /// </summary>
        [Given]
        public
        void
        GivenAQueue()
        {
            var homepage = this.WebBrowser.NavigateTo<Homepage>();

            var manageQueueSection = homepage
                .CreateQueueForm
                .CreateQueue();

            QueueSteps.ContextSet(manageQueueSection);
        }
    }
}