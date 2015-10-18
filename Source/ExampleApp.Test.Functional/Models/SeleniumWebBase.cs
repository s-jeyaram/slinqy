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
        /// <summary>
        /// Reference to the IWebDriver to use for interacting with the web browser.
        /// </summary>
        private readonly IWebDriver _webBrowserDriver;

        /// <summary>
        /// Initializes the SeleniumWebBase with the IWebDriver to use.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the IWebDriver to use for interacting with the web browser.</param>
        protected
        SeleniumWebBase(
            IWebDriver webBrowserDriver)
        {
            _webBrowserDriver = webBrowserDriver;

            PageFactory.InitElements(_webBrowserDriver, this);
        }
    }
}