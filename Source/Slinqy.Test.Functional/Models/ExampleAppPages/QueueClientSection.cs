namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using OpenQA.Selenium;

    /// <summary>
    /// Models the queue client section of the Homepage.
    /// </summary>
    public class QueueClientSection : SeleniumWebBase
    {
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
        /// Generates messages and puts them in the queue.
        /// </summary>
        public
        void
        GenerateQueueMessages()
        {
            this.ToString();
        }
    }
}
