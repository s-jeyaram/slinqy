using System;
using OpenQA.Selenium;

namespace ExampleApp.Test.Functional.Models.ExampleAppPages
{
    /// <summary>
    /// Models the Slinqy Example App Homepage.
    /// </summary>
    public class HomePage : SlinqyExampleWebPage
    {
        /// <summary>
        /// Defines the relative path for the Homepage.
        /// </summary>
        public const string RelativePath = "/";

        /// <summary>
        /// Initializes the HomePage with the IWebDriver to use for controlling the page.
        /// </summary>
        /// <param name="webBrowserDriver">
        /// Specifies the IWebDriver to use to interact with the Homepage.
        /// </param>
        public 
        HomePage(
            IWebDriver webBrowserDriver) 
                : base(webBrowserDriver, new Uri(RelativePath, UriKind.Relative))
        {
        }
    }
}