namespace ExampleApp.Test.Functional.Steps
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Models;
    using Models.ExampleAppPages;
    using System;
    using System.Linq;
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
        private readonly WebBrowser             webBrowser;

        /// <summary>
        /// Maintains a reference to the scenarios contextual information.
        /// </summary>
        private readonly SpecFlowContextualInfo context;

        /// <summary>
        /// Initializes a new instance with a WebBrowser.
        /// </summary>
        /// <param name="context">
        /// Specifies the contextual information about the current scenario.
        /// </param>
        /// <param name="browser">
        /// Specifies the WebBrowser instance to use for interacting with the browser.
        /// </param>
        public 
        QueueSteps(
            SpecFlowContextualInfo  context,
            WebBrowser              browser)
        {
            this.context    = context;
            this.webBrowser = browser;
        }

        /// <summary>
        /// Configures the Queue Storage Utilization Scale Up Threshold setting on the queue agent.
        /// </summary>
        [Given]
        public 
        void 
        GivenTheQueueStorageUtilizationScaleUpThresholdIsSet()
        {
            var homepage = this.webBrowser.NavigateTo<Homepage>();

            // Set the threshold low so it doesn't take much to hit it.
            homepage.ScaleUpThresholdPercentage = 0.05;
            homepage.UpdateAgent();

            ContextSet(homepage);
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
            ContextGet<Homepage>()
                .GenerateQueueMessages();
        }

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
        void 
        ThenTheQueueStorageCapacityExpands()
        {
            var homepage = ContextGet<Homepage>();

            // TODO: Make poll logic a generic function

            var pollMaxSeconds     = 60;
            var pollStartTimestamp = DateTimeOffset.UtcNow;

            while (DateTimeOffset.UtcNow.Subtract(pollStartTimestamp).TotalSeconds <= pollMaxSeconds)
            {
                // Evaluate queue capacity over time, should see it expand since the start of the test.
                var capacityHistory = homepage.QueueHistory.Where(history => history.Timestamp > this.context.ScenarioStartTimestamp);

                var lastCapacityValue = 0;

                // Check if the capacity has increased
                foreach (var historyRecord in capacityHistory)
                {
                    if (lastCapacityValue > 0 && historyRecord.StorageCapacityGigabytes > lastCapacityValue)
                        return;
                    
                    lastCapacityValue = historyRecord.StorageCapacityGigabytes;
                }
            }

            Assert.Fail("The Queue Storage Capacity was not increased as expected.");
        }
    }
}
