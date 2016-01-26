namespace Slinqy.Test.Functional.Utilities.Selenium
{
    using OpenQA.Selenium;

    /// <summary>
    /// Helpful extensions over the IWebElement Selenium interface.
    /// </summary>
    internal static class WebElementExtensions
    {
        /// <summary>
        /// Specifies the full string to perform a Control+a keyboard command to do things like "select all".
        /// </summary>
        private static readonly string ControlA = Keys.Control + "a" + Keys.Control;

        /// <summary>
        /// First selects the contents of the element, if any, by performing a Control+A keyboard operation,
        /// then enters the specified keys to replace the contents, if any.
        /// </summary>
        /// <param name="webElement">The element to send keys to.</param>
        /// <param name="keys">The keys to send to the element.</param>
        public
        static
        void
        SelectAndSendKeys(
            this IWebElement webElement,
            string keys)
        {
            webElement.SendKeys(ControlA + keys);
        }
    }
}
