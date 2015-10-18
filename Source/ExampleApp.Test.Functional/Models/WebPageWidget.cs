using OpenQA.Selenium;

namespace ExampleApp.Test.Functional.Models
{
    /// <summary>
    /// Models a sub-component of a web page.
    /// </summary>
    public class WebPageWidget : SeleniumWebBase
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the driver used for controlling the browser.</param>
        public 
        WebPageWidget(
            IWebDriver webBrowserDriver) : base(webBrowserDriver)
        {
        }
    }
}
