namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using OpenQA.Selenium;

    /// <summary>
    /// Models the Manage Queue section of the Homepage.
    /// </summary>
    public class ManageQueueSection : SeleniumWebBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManageQueueSection"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">
        /// Specifies the driver to use for interacting with the web browser.
        /// </param>
        public
        ManageQueueSection(
            IWebDriver          webBrowserDriver)
                : base(webBrowserDriver)
        {
            this.QueueInformation = new QueueInformationSection(webBrowserDriver);
            this.QueueClient      = new QueueClientSection(webBrowserDriver);
        }

        /// <summary>
        /// Gets the static queue information.
        /// </summary>
        public QueueInformationSection     QueueInformation    { get; private set; }

        /// <summary>
        /// Gets the queue client section for interacting with the queue.
        /// </summary>
        public QueueClientSection   QueueClient         { get; private set; }
    }
}
