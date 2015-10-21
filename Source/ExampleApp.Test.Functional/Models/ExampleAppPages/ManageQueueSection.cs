namespace ExampleApp.Test.Functional.Models.ExampleAppPages
{
    using OpenQA.Selenium;

    /// <summary>
    /// Models the Manage Queue section of the Homepage.
    /// </summary>
    public class ManageQueueSection : SeleniumWebBase
    {
        /// <summary>
        /// Gets the static queue information.
        /// </summary>
        public QueueInformation QueueInformation { get; private set; }

        /// <summary>
        /// Gets the queue client section for interacting with the queue.
        /// </summary>

        public QueueClientSection QueueClient { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="webBrowserDriver">
        /// Specifies the driver to use for interacting with the web browser.
        /// </param>
        public 
        ManageQueueSection(
            IWebDriver          webBrowserDriver) : base(webBrowserDriver)
        {
            this.QueueInformation = new QueueInformation(webBrowserDriver);
            this.QueueClient      = new QueueClientSection(webBrowserDriver);
        }
    }
}
