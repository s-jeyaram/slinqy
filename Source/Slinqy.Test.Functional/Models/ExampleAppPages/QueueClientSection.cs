namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System;
    using System.Globalization;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.PageObjects;
    using Utilities.Polling;
    using Utilities.Strings;

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
        /// A proxy reference to the element in the web browser.
        /// </summary>
        /// <remarks>This field is automatically populated by SpecFlow.</remarks>
        [FindsBy(How = How.Id, Using = "MessageBody")]
        private IWebElement messageBodyInput = null;

        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        /// <remarks>This field is automatically populated by SpecFlow.</remarks>
        [FindsBy(How = How.Id, Using = "SendMessageButton")]
        private IWebElement sendMessageButton = null;

        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        /// <remarks>This field is automatically populated by SpecFlow.</remarks>
        [FindsBy(How = How.Id, Using = "ReceiveMessageButton")]
        private IWebElement receiveMessageButton = null;

        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        /// <remarks>This field is automatically populated by SpecFlow.</remarks>
        [FindsBy(How = How.Id, Using = "ReceivedMessageBody")]
        private IWebElement receivedMessageBody = null;

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
            Poll.Value(
                from:               () => this.fillQueueButton.Enabled,
                until:              enabled => enabled,
                interval:           TimeSpan.FromMilliseconds(500),
                maxPollDuration:    TimeSpan.FromSeconds(30)
            );

            // Get the # of messages from the UI.
            return int.Parse(this.fillQueueMessagesSent.Text, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Submits a request to the server to receive any messages from the queue and waits for the queue to be empty.
        /// </summary>
        /// <returns>Returns the count of received messages from the UI.</returns>
        public
        int
        ReceiveQueue()
        {
            this.receiveQueueButton.Click();

            // Wait for it to finish
            Poll.Value(
                from:               () => this.receiveQueueButton.Enabled,
                until:              enabled => enabled,
                interval:           TimeSpan.FromMilliseconds(500),
                maxPollDuration:    TimeSpan.FromSeconds(15)
            );

            // Get the # of messages from the UI.
            return int.Parse(this.receiveQueueMessagesReceived.Text, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Sends a randomly generated message to the queue.
        /// </summary>
        /// <returns>Returns the body of the randomly generated message.</returns>
        public
        string
        SendMessage()
        {
            var randomMessage = StringUtilities.RandomString(10);

            this.messageBodyInput.SendKeys(randomMessage);

            this.sendMessageButton.Click();

            // Wait for it to finish
            Poll.Value(
                from:               () => this.sendMessageButton.Enabled,
                until:              enabled => enabled,
                interval:           TimeSpan.FromMilliseconds(500),
                maxPollDuration:    TimeSpan.FromSeconds(15)
            );

            return randomMessage;
        }

        /// <summary>
        /// Receives a single message from the queue.
        /// </summary>
        /// <returns>Returns the body of the queue message that was received.</returns>
        public
        string
        ReceiveQueueMessage()
        {
            this.receiveMessageButton.Click();

            // Wait for it to finish
            Poll.Value(
                from:               () => this.receivedMessageBody.Text,
                until:              body => !string.IsNullOrWhiteSpace(body),
                interval:           TimeSpan.FromMilliseconds(500),
                maxPollDuration:    TimeSpan.FromSeconds(15)
            );

            return this.receivedMessageBody.Text;
        }
    }
}
