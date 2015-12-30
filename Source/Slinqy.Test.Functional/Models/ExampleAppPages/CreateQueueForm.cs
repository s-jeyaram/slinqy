namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.PageObjects;
    using Strings;

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
            const int OneGb      = 1024;
            const int OnePercent = 1;

            var name = "test-" + StringUtilities.RandomString(4);

            var createParams = new CreateQueueParameters(
                queueName:                  name,
                storageCapacityMegabytes:   OneGb,
                scaleUpThresholdPercentage: OnePercent
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

            // Enter parameters in to form.
            this.queueName.Clear();
            this.queueName.SendKeys(createQueueParameters.QueueName);
            this.maxQueueSizeMegabytes.Clear();
            this.maxQueueSizeMegabytes.SendKeys(createQueueParameters.StorageCapacityMegabytes.ToString(CultureInfo.InvariantCulture));
            this.storageCapacityScaleOutThresholdPercentage.Clear();
            this.storageCapacityScaleOutThresholdPercentage.SendKeys(createQueueParameters.ScaleUpThresholdPercentage.ToString(CultureInfo.InvariantCulture));

            // Submit
            this.createQueueButton.Click();

            // Wait for it to finish
            while (this.ajaxStatus.Text != "COMPLETED")
                Thread.Sleep(500);

            // Check the result
            if (this.ajaxResult.Text == "FAILED")
                throw new InvalidOperationException(this.ajaxStatusMessage.Text);

            // Return queue info
            return new ManageQueueSection(this.WebBrowserDriver);
        }
    }
}