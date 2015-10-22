namespace Slinqy.Test.Functional.Models
{
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.PageObjects;

    /// <summary>
    /// Models a web component that can be initialized by Selenium.
    /// A web component can either be an entire web page, or part of a web page.
    /// </summary>
    public abstract class SeleniumWebBase
    {
        /// <summary>
        /// Gets the driver for the web browser.
        /// </summary>
        protected IWebDriver WebBrowserDriver { get; private set; }

        /// <summary>
        /// Initializes the SeleniumWebBase with the IWebDriver to use.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the IWebDriver to use for interacting with the web browser.</param>
        protected
        SeleniumWebBase(
            IWebDriver webBrowserDriver)
        {
            this.WebBrowserDriver = webBrowserDriver;

            PageFactory.InitElements(this.WebBrowserDriver, this);
        }
    }
}