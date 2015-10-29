namespace Slinqy.Test.Functional.Models
{
    using System;
    using OpenQA.Selenium;

    /// <summary>
    /// Models a whole web page.
    /// </summary>
    public abstract class Webpage : SeleniumWebBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Webpage"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the driver to use for controlling the browser.</param>
        /// <param name="webpageRelativePath">Specifies the relative path of this web page.</param>
        protected
        Webpage(
            IWebDriver  webBrowserDriver,
            Uri         webpageRelativePath)
                : base(webBrowserDriver)
        {
            this.WebpageRelativeUri = webpageRelativePath;
        }

        /// <summary>
        /// Gets the relative path of the web page.
        /// </summary>
        public Uri  WebpageRelativeUri  { get; private set; }
    }
}