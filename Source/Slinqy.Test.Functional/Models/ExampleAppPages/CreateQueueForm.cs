namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System;
    using System.Globalization;
    using System.Threading;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.PageObjects;
    using Utilities.Strings;

    /// <summary>
    /// Models the form for creating a new queue.
    /// </summary>
    public class CreateQueueForm : SeleniumWebBase
    {
        /// <summary>
        /// The proxy reference to the AJAX request result message element on the web page.
        /// </summary>
        private readonly AjaxIndicatorSection createQueueAjaxStatusSection;

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
        /// The proxy reference to the AJAX request result message element on the web page.
        /// </summary>
        [FindsBy(How = How.Id, Using = "CreateQueueAjaxStatus")]
        private IWebElement createQueueAjaxStatusSectionElement = null;

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
            this.createQueueAjaxStatusSection = new AjaxIndicatorSection(
                webBrowserDriver,
                this.createQueueAjaxStatusSectionElement
            );
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

            // Clear values to avoid appending to whatever may already be there.
            this.queueName.Clear();
            this.maxQueueSizeMegabytes.Clear();
            this.storageCapacityScaleOutThresholdPercentage.Clear();

            // Enter parameters in to form.
            this.queueName.SendKeys(createQueueName);
            this.maxQueueSizeMegabytes.SendKeys(createQueueParameters.StorageCapacityMegabytes.ToString(CultureInfo.InvariantCulture));
            this.storageCapacityScaleOutThresholdPercentage.SendKeys(createQueueParameters.ScaleUpThresholdPercentage.ToString(CultureInfo.InvariantCulture));

            // Submit
            this.createQueueButton.Click();

            // Wait for it to finish
            this.createQueueAjaxStatusSection.WaitForResult(maxDuration: TimeSpan.FromSeconds(15));

            // Return queue info
            return new ManageQueueSection(this.WebBrowserDriver);
        }
    }
}