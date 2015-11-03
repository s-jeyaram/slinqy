namespace Slinqy.Test.Functional.Models
{
    using OpenQA.Selenium;

    /// <summary>
    /// Models a sub-component of a web page.
    /// </summary>
    public class WebpageWidget : SeleniumWebBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebpageWidget"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the driver used for controlling the browser.</param>
        public
        WebpageWidget(
            IWebDriver webBrowserDriver)
                : base(webBrowserDriver)
        {
        }
    }
}
