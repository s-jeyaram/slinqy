using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace ExampleApp.Test.Functional.Models.ExampleAppPages
{
    public class WebsiteFooter : WebPageWidget
    {
        [FindsBy] private IWebElement AppVersion = null;

        public string Version { get { return AppVersion.Text; } }

        public 
        WebsiteFooter(
            IWebDriver webBrowserDriver) : base(webBrowserDriver)
        {
        }
    }
}