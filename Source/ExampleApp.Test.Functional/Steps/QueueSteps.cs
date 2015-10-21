namespace ExampleApp.Test.Functional.Steps
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Models;
    using Models.ExampleAppPages;
    using System;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Defines steps for working with the queues of the Example App.
    /// </summary>
    [Binding]
    public class QueueSteps
    {
        /// <summary>
        /// Maintains a reference to the WebBrowser driver to use for interacting with the web browser.
        /// </summary>
        private readonly WebBrowser webBrowser;
        
        /// <summary>
        /// Initializes a new instance with a WebBrowser.
        /// </summary>
        /// <param name="browser">
        /// Specifies the WebBrowser instance to use for interacting with the browser.
        /// </param>
        public 
        QueueSteps(
            WebBrowser browser)
        {
            this.webBrowser = browser;
        }

        /// <summary>
        /// Creates a Queue with the Queue Storage Utilization Scale Up Threshold setting.
        /// </summary>
        [Given]
        public 
        void 
        GivenAQueueWithStorageUtilizationScaleUpThresholdSet()
        {
            var homepage = this.webBrowser.NavigateTo<Homepage>();

            // Configure so it doesn't take much to hit the threshold.
            var createQueueParams = new CreateQueueParameters(
                "test-queue",
                storageCapacityMegabytes:   1024,
                scaleUpThresholdPercentage: 0.05
            );

            var manageQueueSection = homepage
                .CreateQueueForm
                .CreateQueue(createQueueParams);
            
            ContextSet(createQueueParams);
            ContextSet(manageQueueSection);
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
                .GenerateQueueMessages();
        }

        // TODO: Move these Context methods to a base class.

        /// <summary>
        /// Saves the specified value in the scenario context for subsequent steps to use.
        /// </summary>
        /// <param name="value">Specifies the value to save.</param>
        /// <typeparam name="T">Specifies the type of the value.</typeparam>
        private
        static
        void
        ContextSet<T>(
            T value)
        {
            ScenarioContext.Current.Set(value);
        }

        /// <summary>
        /// Retrieves a value that was saved by a previous step.
        /// </summary>
        /// <typeparam name="T">Specifies the type of the value.</typeparam>
        /// <returns>Returns the requested value.</returns>
        private 
        static 
        T 
        ContextGet<T>()
        {
            return ScenarioContext.Current.Get<T>();
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
            var createQueueParams = ContextGet<CreateQueueParameters>();

            // TODO: Make poll logic a generic function

            var pollMaxSeconds     = 60;
            var pollStartTimestamp = DateTimeOffset.UtcNow;

            while (DateTimeOffset.UtcNow.Subtract(pollStartTimestamp).TotalSeconds <= pollMaxSeconds)
            {
                if (manageQueueSection.QueueInformation.StorageCapacityMegabytes > createQueueParams.StorageCapacityMegabytes)
                    return;
            }

            Assert.Fail("The Queue Storage Capacity was not increased as expected.");
        }
    }
}