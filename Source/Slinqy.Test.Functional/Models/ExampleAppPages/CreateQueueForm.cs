namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System;
    using System.Globalization;
    using System.Threading;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.PageObjects;

    /// <summary>
    /// Models the form for creating a new queue.
    /// </summary>
    public class CreateQueueForm : SeleniumWebBase
    {
        /// <summary>
        /// The proxy reference to the queue name input on the web page.
        /// </summary>
        [FindsBy] private IWebElement QueueName = null;

        /// <summary>
        /// The proxy reference to the queue storage capacity input on the web page.
        /// </summary>
        [FindsBy] private IWebElement MaxQueueSizeMegabytes = null;

        /// <summary>
        /// The proxy reference to the submit button on the create queue form.
        /// </summary>
        [FindsBy] private IWebElement CreateQueueButton = null;

        /// <summary>
        /// The proxy reference to the AJAX request status element on the web page.
        /// </summary>
        [FindsBy] private IWebElement AjaxStatus = null;

        /// <summary>
        /// The proxy reference to the AJAX request result element on the web page.
        /// </summary>
        [FindsBy] private IWebElement AjaxResult = null;

        /// <summary>
        /// The proxy reference to the AJAX request result message element on the web page.
        /// </summary>
        [FindsBy] private IWebElement AjaxStatusMessage = null;

        /// <summary>
        /// Initializes a new instance with the web browser driver.
        /// </summary>
        /// <param name="webBrowserDriver">
        /// Specifies the driver to use for interacting with the web browser.
        /// </param>
        public 
        CreateQueueForm(
            IWebDriver webBrowserDriver) : base(webBrowserDriver)
        {
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
                throw new ArgumentNullException("createQueueParameters");

            // Enter parameters in to form.
            this.QueueName.SendKeys(createQueueParameters.QueueName);
            this.MaxQueueSizeMegabytes.SendKeys(createQueueParameters.StorageCapacityMegabytes.ToString(CultureInfo.InvariantCulture));

            // Submit
            this.CreateQueueButton.Click();

            // Wait for it to finish
            while (this.AjaxStatus.Text != "COMPLETED")
                Thread.Sleep(1000);

            // Check the result
            if (this.AjaxResult.Text == "FAILED")
                throw new InvalidOperationException(this.AjaxStatusMessage.Text);
        
            // Return queue info
            return new ManageQueueSection(this.WebBrowserDriver);
        }
    }
}