namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.PageObjects;

    /// <summary>
    /// Models the footer of the Slinqy Example App website.
    /// </summary>
    public class WebsiteFooterSection : WebpageWidget
    {
        /// <summary>
        /// Proxy to the web element containing the application version number.
        /// </summary>
        /// <remarks>This field is automatically populated by Selenium.</remarks>
        [FindsBy(How = How.Id, Using = "AppVersion")] private IWebElement appVersion = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteFooterSection"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the IWebDriver to use for controlling the browser.</param>
        public
        WebsiteFooterSection(
            IWebDriver webBrowserDriver)
                : base(webBrowserDriver)
        {
        }

        /// <summary>
        /// Gets the version number displayed on the web page footer.
        /// </summary>
        public string   Version => this.appVersion.Text;
    }
}