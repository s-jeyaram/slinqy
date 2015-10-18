using System;
using OpenQA.Selenium;

namespace ExampleApp.Test.Functional.Models.ExampleAppPages
{
    /// <summary>
    /// The base class for all Slinqy Example App web pages.
    /// </summary>
    public abstract class SlinqyExampleWebPage : WebPage
    {
        /// <summary>
        /// Gets the Footer portion of the web page.
        /// </summary>
        public WebsiteFooter Footer { get; private set; }

        /// <summary>
        /// Initializes the SlinqyExampleWebPage with the web diver and relative path of the web page.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the IWebDriver to use for controlling the browser.</param>
        /// <param name="webpageRelativePath">Specifies the relative path of the web page.</param>
        protected SlinqyExampleWebPage(
            IWebDriver  webBrowserDriver,
            Uri         webpageRelativePath) : base(webBrowserDriver, webpageRelativePath)
        {
            Footer = new WebsiteFooter(webBrowserDriver);
        }
    }
}