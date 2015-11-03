namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System.Globalization;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.PageObjects;

    /// <summary>
    /// Models the queue information that displays on the Homepage.
    /// </summary>
    public class QueueInformationSection : SeleniumWebBase
    {
        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        /// <remarks>This field is automatically populated by SpecFlow.</remarks>
        [FindsBy(How = How.Id, Using = "QueueInformation_QueueName")] private IWebElement queueName = null;

        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        [FindsBy(How = How.Id, Using = "QueueInformation_StorageCapacityMegabytes")] private IWebElement storageCapacityMegabytes = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueInformationSection"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">
        /// Specifies the driver to use for interacting with the web browser.
        /// </param>
        public
        QueueInformationSection(
            IWebDriver webBrowserDriver)
                : base(webBrowserDriver)
        {
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        public string   QueueName                   => this.queueName.Text;

        /// <summary>
        /// Gets the storage capacity of the queue.
        /// </summary>
        public int      StorageCapacityMegabytes    => int.Parse(this.storageCapacityMegabytes.Text, CultureInfo.InvariantCulture);
    }
}
