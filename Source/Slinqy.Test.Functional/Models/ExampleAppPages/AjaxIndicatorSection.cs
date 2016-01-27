namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System;
    using System.Linq;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Support.UI;

    /// <summary>
    /// Models the ajax indicator widget.
    /// </summary>
    public class AjaxIndicatorSection : SeleniumWebBase
    {
        /// <summary>
        /// The CSS ID of the AJAX Status Element.
        /// </summary>
        private readonly string ajaxStatusElementId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxIndicatorSection"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the web driver to use for interacting with the web browser.</param>
        /// <param name="ajaxStatusElementId">Specifies the ID of the element where the AJAX status will be rendered.</param>
        public
        AjaxIndicatorSection(
            IWebDriver  webBrowserDriver,
            string      ajaxStatusElementId)
            : base(webBrowserDriver)
        {
            this.ajaxStatusElementId = ajaxStatusElementId;
        }

        /// <summary>
        /// Waits for the indicator to report the result of the AJAX request.
        /// </summary>
        /// <param name="maxDuration">
        /// Specifies how long to wait before timing out.
        /// </param>
        public
        void
        WaitForResult(
            TimeSpan maxDuration)
        {
            new WebDriverWait(
                this.WebBrowserDriver,
                maxDuration
            ).Until(
                driver =>
                {
                    var statusElement = driver.FindElements(
                        By.Id(this.ajaxStatusElementId)
                    ).SingleOrDefault();

                    if (statusElement == null)
                        return true; // The status element is no longer on the page.

                    var statusClass = statusElement.GetAttribute("class");

                    if (statusClass.Contains("ajax-failed"))
                        throw new InvalidOperationException(statusElement.Text);

                    return !statusClass.Contains("ajax-running");
                }

            );
        }
    }
}