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
        /// The proxy reference to the ajax indicator element on the web page.
        /// </summary>
        private readonly IWebElement ajaxStatusElement;

        /// <summary>
        /// The CSS ID of the AJAX Status Element.
        /// </summary>
        private readonly string ajaxStatusElementId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxIndicatorSection"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">Specifies the web driver to use for interacting with the web browser.</param>
        /// <param name="ajaxStatusElement">Specifies the element where the AJAX status will be rendered.</param>
        public
        AjaxIndicatorSection(
            IWebDriver  webBrowserDriver,
            IWebElement ajaxStatusElement)
            : base(webBrowserDriver)
        {
            if (ajaxStatusElement == null)
                throw new ArgumentNullException(nameof(ajaxStatusElement));

            this.ajaxStatusElement = ajaxStatusElement;
            this.ajaxStatusElementId = ajaxStatusElement.GetAttribute("id");
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
                    var statusStillExists = driver.FindElements(
                        By.Id(this.ajaxStatusElementId)
                    ).Any();

                    if (!statusStillExists)
                        return true; // The status element is no longer on the page.

                    var statusClass = this.ajaxStatusElement.GetAttribute("class");

                    if (statusClass.Contains("ajax-failed"))
                        throw new InvalidOperationException(this.ajaxStatusElement.Text);

                    return !statusClass.Contains("ajax-running");
                }

            );
        }
    }
}