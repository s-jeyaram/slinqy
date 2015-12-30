namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System;
    using System.Globalization;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.PageObjects;
    using Utilities.Polling;
    using Utilities.Strings;

    /// <summary>
    /// Models the form for creating a new queue.
    /// </summary>
    public class CreateQueueForm : SeleniumWebBase
    {
        /// <summary>
        /// The proxy reference to the queue name input on the web page.
        /// </summary>
        [FindsBy(How = How.Id, Using = "QueueName")]
        private IWebElement queueName = null;

        /// <summary>
        /// The proxy reference to the queue storage capacity input on the web page.
        /// </summary>
        [FindsBy(How = How.Id, Using = "MaxQueueSizeMegabytes")]
        private IWebElement maxQueueSizeMegabytes = null;

        /// <summary>
        /// The proxy reference to the scale out threshold input on the web page.
        /// </summary>
        [FindsBy(How = How.Id, Using = "StorageCapacityScaleOutThresholdPercentage")]
        private IWebElement storageCapacityScaleOutThresholdPercentage = null;

        /// <summary>
        /// The proxy reference to the submit button on the create queue form.
        /// </summary>
        [FindsBy(How = How.Id, Using = "CreateQueueButton")]
        private IWebElement createQueueButton = null;

        /// <summary>
        /// The proxy reference to the AJAX request status element on the web page.
        /// </summary>
        [FindsBy(How = How.Id, Using = "AjaxStatus")]
        private IWebElement ajaxStatus = null;

        /// <summary>
        /// The proxy reference to the AJAX request result element on the web page.
        /// </summary>
        [FindsBy(How = How.Id, Using = "AjaxResult")]
        private IWebElement ajaxResult = null;

        /// <summary>
        /// The proxy reference to the AJAX request result message element on the web page.
        /// </summary>
        [FindsBy(How = How.Id, Using = "AjaxStatusMessage")]
        private IWebElement ajaxStatusMessage = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQueueForm"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">
        /// Specifies the driver to use for interacting with the web browser.
        /// </param>
        public
        CreateQueueForm(
            IWebDriver webBrowserDriver)
                : base(webBrowserDriver)
        {
        }

        /// <summary>
        /// Creates a new queue with all default minimal settings.
        /// </summary>
        /// <returns>Returns the queue management section that appears after creating a new queue.</returns>
        public
        ManageQueueSection
        CreateQueue()
        {
            var createParams = new CreateQueueParameters(
                queueName:                  "test",
                storageCapacityMegabytes:   1024,
                scaleUpThresholdPercentage: 1,
                randomizeQueueName:         true
            );

            return this.CreateQueue(createParams);
        }

        /// <summary>
        /// Creates a new queue with the specified parameters.
        /// </summary>
        /// <param name="createQueueParameters">Specifies the parameters for the new queue.</param>
        /// <returns>Returns the queue management section that appears after creating a new queue.</returns>
        public ManageQueueSection CreateQueue(
            CreateQueueParameters createQueueParameters)
        {
            if (createQueueParameters == null)
                throw new ArgumentNullException(nameof(createQueueParameters));

            var createQueueName = createQueueParameters.QueueName;

            if (createQueueParameters.RandomizeQueueName)
                createQueueName += StringUtilities.RandomString(4);

            // Enter parameters in to form.
            this.queueName.Clear();
            this.queueName.SendKeys(createQueueName);
            this.maxQueueSizeMegabytes.Clear();
            this.maxQueueSizeMegabytes.SendKeys(createQueueParameters.StorageCapacityMegabytes.ToString(CultureInfo.InvariantCulture));
            this.storageCapacityScaleOutThresholdPercentage.Clear();
            this.storageCapacityScaleOutThresholdPercentage.SendKeys(createQueueParameters.ScaleUpThresholdPercentage.ToString(CultureInfo.InvariantCulture));

            // Submit
            this.createQueueButton.Click();

            // Wait for it to finish
            Poll.Value(
                from:               ()   => this.ajaxStatus.Text,
                until:              text => text != "COMPLETED",
                maxDuration:    TimeSpan.FromSeconds(15),
                interval:           TimeSpan.FromMilliseconds(500)
            );

            // Check the result
            if (this.ajaxResult.Text == "FAILED")
                throw new InvalidOperationException(this.ajaxStatusMessage.Text);

            // Return queue info
            return new ManageQueueSection(this.WebBrowserDriver);
        }
    }
}