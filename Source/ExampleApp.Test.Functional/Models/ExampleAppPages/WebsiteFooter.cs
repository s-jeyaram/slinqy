using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace ExampleApp.Test.Functional.Models.ExampleAppPages
{
    /// <summary>
    /// Models the footer of the Slinqy Example App website.
    /// </summary>
    public class WebsiteFooter : WebPageWidget
    {
        /// <summary>
        /// Proxy to the web element containing the application version number.
        /// </summary>
        /// <remarks>This field is automatically populated by Selenium.</remarks>
        [FindsBy] private IWebElement AppVersion = null;

        /// <summary>
        /// Gets the version number displayed on the web page footer.
        /// </summary>
        public string Version { get { return AppVersion.Text; } }

        /// <summary>
        /// Initializes the class with the IWebDriver to use for controlling the browser.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the IWebDriver to use for controlling the browser.</param>
        public 
        WebsiteFooter(
            IWebDriver webBrowserDriver) : base(webBrowserDriver)
        {
        }
    }
}