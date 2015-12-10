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
                scaleUpThresholdPercentage: 0.05
            );

            var manageQueueSection = homepage
                .CreateQueueForm
                .CreateQueue(createQueueParams);

            QueueSteps.ContextSet(createQueueParams);
            QueueSteps.ContextSet(manageQueueSection);
        }
    }
}