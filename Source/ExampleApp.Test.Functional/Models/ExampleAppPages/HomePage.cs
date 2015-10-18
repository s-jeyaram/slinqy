using System;
using OpenQA.Selenium;

namespace ExampleApp.Test.Functional.Models.ExampleAppPages
{
    public class HomePage : SlinqyExampleWebPage
    {
        public const string RelativePath = "/";

        public 
        HomePage(
            IWebDriver webBrowserDriver) : base(webBrowserDriver, new Uri(RelativePath, UriKind.Relative))
        {
        }
    }
}
