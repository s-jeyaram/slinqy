namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System.Globalization;
    using System.Threading;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.PageObjects;

    /// <summary>
    /// Models the queue client section of the Homepage.
    /// </summary>
    public class QueueClientSection : SeleniumWebBase
    {
        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        /// <remarks>This field is automatically populated by SpecFlow.</remarks>
        [FindsBy(How = How.Id, Using = "SizeMegabytes")]
        private IWebElement sizeMegabytesInput = null;

        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        /// <remarks>This field is automatically populated by SpecFlow.</remarks>
        [FindsBy(How = How.Id, Using = "StartFillingQueueButton")]
        private IWebElement fillQueueButton = null;

        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        /// <remarks>This field is automatically populated by SpecFlow.</remarks>
        [FindsBy(How = How.Id, Using = "StartReceivingQueueButton")]
        private IWebElement receiveQueueButton = null;

        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        /// <remarks>This field is automatically populated by SpecFlow.</remarks>
        [FindsBy(How = How.Id, Using = "FillQueueMessagesSent")]
        private IWebElement fillQueueMessagesSent = null;

        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        /// <remarks>This field is automatically populated by SpecFlow.</remarks>
        [FindsBy(How = How.Id, Using = "ReceiveQueueMessagesReceived")]
        private IWebElement receiveQueueMessagesReceived = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueClientSection"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the driver to use for interacting with the web browser.</param>
        public
        QueueClientSection(
            IWebDriver webBrowserDriver)
                : base(webBrowserDriver)
        {
        }

        /// <summary>
        /// Submits a request to the server to fill the queue.
        /// </summary>
        public
        void
        FillQueue()
        {
            this.FillQueue(25);
        }

        /// <summary>
        /// Submits a request to the server to fill the queue and waits for the operation to complete.
        /// </summary>
        /// <param name="sizeMegabytes">Specifies how much data to send to the queue.</param>
        /// <returns>Returns the number of messages that were sent.</returns>
        public
        int
        FillQueue(
            int sizeMegabytes)
        {
            this.sizeMegabytesInput.Clear();
            this.sizeMegabytesInput.SendKeys(sizeMegabytes.ToString(CultureInfo.InvariantCulture));

            this.fillQueueButton.Click();

            // Wait for it to finish
            while (!this.fillQueueButton.Enabled)
                Thread.Sleep(500);

            // Get the # of messages from the UI.
            return int.Parse(this.fillQueueMessagesSent.Text, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Submits a request to the server to receive any messages from the queue and waits for the queue to be empty.
        /// </summary>
        /// <returns>Returns the count of received messages from the UI.</returns>
        public
        int
        ReadQueue()
        {
            this.receiveQueueButton.Click();

            // Wait for it to finish
            while (!this.receiveQueueButton.Enabled)
                Thread.Sleep(500);

            // Get the # of messages from the UI.
            return int.Parse(this.receiveQueueMessagesReceived.Text, CultureInfo.InvariantCulture);
        }
    }
}
