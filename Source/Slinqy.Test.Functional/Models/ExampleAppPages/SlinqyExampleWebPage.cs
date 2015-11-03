namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System;
    using OpenQA.Selenium;

    /// <summary>
    /// The base class for all Slinqy Example App web pages.
    /// </summary>
    public abstract class SlinqyExampleWebpage : Webpage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyExampleWebpage"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the IWebDriver to use for controlling the browser.</param>
        /// <param name="webpageRelativePath">Specifies the relative path of the web page.</param>
        protected
        SlinqyExampleWebpage(
            IWebDriver webBrowserDriver,
            Uri        webpageRelativePath)
                : base(webBrowserDriver, webpageRelativePath)
        {
            this.Footer = new WebsiteFooterSection(webBrowserDriver);
        }

        /// <summary>
        /// Gets the Footer portion of the web page.
        /// </summary>
        public WebsiteFooterSection    Footer  { get; private set; }
    }
}