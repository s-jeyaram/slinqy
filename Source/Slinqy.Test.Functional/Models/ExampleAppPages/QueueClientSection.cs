namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System.Globalization;
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
        /// Submits a request to the server to fill the queue.
        /// </summary>
        /// <param name="sizeMegabytes">Specifies how much data to send to the queue.</param>
        public
        void
        FillQueue(
            int sizeMegabytes)
        {
            this.sizeMegabytesInput.Clear();
            this.sizeMegabytesInput.SendKeys(sizeMegabytes.ToString(CultureInfo.InvariantCulture));

            this.fillQueueButton.Click();

            // Wait for it to finish
        }
    }
}
