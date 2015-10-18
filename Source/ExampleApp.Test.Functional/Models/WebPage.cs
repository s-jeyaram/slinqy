using System;
using OpenQA.Selenium;

namespace ExampleApp.Test.Functional.Models
{
    /// <summary>
    /// Models a whole web page.
    /// </summary>
    public abstract class WebPage : SeleniumWebBase
    {
        /// <summary>
        /// Gets the relative path of the web page.
        /// </summary>
        public Uri WebPageRelativeUri { get; private set; }

        /// <summary>
        /// Initializes a new instance with the web driver and relative path of the web page.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the driver to use for controlling the browser.</param>
        /// <param name="webpageRelativePath">Specifies the relative path of this web page.</param>
        protected WebPage(
            IWebDriver  webBrowserDriver,
            Uri         webpageRelativePath) : base(webBrowserDriver)
        {
            WebPageRelativeUri = webpageRelativePath;
        }
    }
}