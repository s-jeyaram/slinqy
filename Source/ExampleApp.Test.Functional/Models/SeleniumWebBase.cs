using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;

namespace ExampleApp.Test.Functional.Models
{
    /// <summary>
    /// Models a web component that can be initialized by Selenium.
    /// A web component can either be an entire web page, or part of a web page.
    /// </summary>
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