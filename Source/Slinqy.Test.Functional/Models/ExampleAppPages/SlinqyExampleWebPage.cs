namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using OpenQA.Selenium;
    using System;

    /// <summary>
    /// The base class for all Slinqy Example App web pages.
    /// </summary>
    public abstract class SlinqyExampleWebpage : Webpage
    {
        /// <summary>
        /// Initializes the SlinqyExampleWebpage with the web diver and relative path of the web page.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the IWebDriver to use for controlling the browser.</param>
        /// <param name="webpageRelativePath">Specifies the relative path of the web page.</param>
        protected 
        SlinqyExampleWebpage(
            IWebDriver webBrowserDriver,
            Uri        webpageRelativePath) : base(webBrowserDriver, webpageRelativePath)
        {
            this.Footer = new WebsiteFooterSection(webBrowserDriver);
        }

        /// <summary>
        /// Gets the Footer portion of the web page.
        /// </summary>
        public WebsiteFooterSection    Footer  { get; private set; }
    }
}