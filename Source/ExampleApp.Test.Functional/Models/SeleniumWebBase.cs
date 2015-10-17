using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace ExampleApp.Test.Functional.Models
{
    public abstract class SeleniumWebBase
    {
        private readonly IWebDriver _webBrowserDriver;

        protected 
        SeleniumWebBase(
            IWebDriver webBrowserDriver)
        {
            _webBrowserDriver = webBrowserDriver;

            PageFactory.InitElements(_webBrowserDriver, this);
        }
    }
}