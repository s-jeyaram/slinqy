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
        [FindsBy] private IWebElement QueueInformation_QueueName = null;

        /// <summary>
        /// A proxy reference to the element in the web browser.
        /// </summary>
        [FindsBy] private IWebElement QueueInformation_StorageCapacityMegabytes = null;
        
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="webBrowserDriver">
        /// Specifies the driver to use for interacting with the web browser.
        /// </param>
        public 
        QueueInformationSection(
            IWebDriver webBrowserDriver) : base(webBrowserDriver)
        {
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        public string   QueueName                   { get { return this.QueueInformation_QueueName.Text; } }

        /// <summary>
        /// Gets the storage capacity of the queue.
        /// </summary>
        public int      StorageCapacityMegabytes    { get { return int.Parse(this.QueueInformation_StorageCapacityMegabytes.Text, CultureInfo.InvariantCulture); } }
    }
}
