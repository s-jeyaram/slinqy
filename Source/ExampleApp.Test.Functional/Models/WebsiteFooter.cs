using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace ExampleApp.Test.Functional.Models
{
    public class WebsiteFooter : WebPageWidget
    {
        [FindsBy] private IWebElement AppVersion = null;

        public string Version => AppVersion.Text;

        public 
        WebsiteFooter(
            IWebDriver webBrowserDriver) : base(webBrowserDriver)
        {
        }
    }
}
