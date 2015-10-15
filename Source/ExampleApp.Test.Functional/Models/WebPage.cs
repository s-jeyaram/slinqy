using System;
using OpenQA.Selenium;

namespace ExampleApp.Test.Functional.Models
{
    /// <summary>
    /// Models a whole web page.
    /// </summary>
    public abstract class WebPage : SeleniumWebBase
    {
        public Uri WebPageRelativeUri { get; private set; }

        protected WebPage(
            IWebDriver  webBrowserDriver,
            Uri         webpageRelativePath) : base(webBrowserDriver)
        {
            WebPageRelativeUri = webpageRelativePath;
        }
    }
}