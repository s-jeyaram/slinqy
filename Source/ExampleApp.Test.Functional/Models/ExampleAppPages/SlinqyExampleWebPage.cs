using System;
using OpenQA.Selenium;

namespace ExampleApp.Test.Functional.Models.ExampleAppPages
{
    public abstract class SlinqyExampleWebPage : WebPage
    {
        public WebsiteFooter Footer { get; private set; }

        protected SlinqyExampleWebPage(
            IWebDriver  webBrowserDriver,
            Uri         webpageRelativePath) : base(webBrowserDriver, webpageRelativePath)
        {
            Footer = new WebsiteFooter(webBrowserDriver);
        }
    }
}