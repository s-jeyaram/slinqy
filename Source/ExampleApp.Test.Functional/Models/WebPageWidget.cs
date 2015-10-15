using OpenQA.Selenium;

namespace ExampleApp.Test.Functional.Models
{
    /// <summary>
    /// Models a sub-component of a web page.
    /// </summary>
    public class WebPageWidget : SeleniumWebBase
    {
        public 
        WebPageWidget(
            IWebDriver webBrowserDriver) : base(webBrowserDriver)
        {
        }
    }
}
