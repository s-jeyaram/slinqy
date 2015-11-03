namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    using System;
    using OpenQA.Selenium;

    /// <summary>
    /// Models the Slinqy Example App Homepage.
    /// </summary>
    public class Homepage : SlinqyExampleWebpage
    {
        /// <summary>
        /// Defines the relative path for the Homepage.
        /// </summary>
        public const string RelativePath = "/";

        /// <summary>
        /// Initializes a new instance of the <see cref="Homepage"/> class.
        /// </summary>
        /// <param name="webBrowserDriver">
        /// Specifies the IWebDriver to use to interact with the Homepage.
        /// </param>
        public
        Homepage(
            IWebDriver webBrowserDriver)
                : base(webBrowserDriver, new Uri(RelativePath, UriKind.Relative))
        {
            this.CreateQueueForm = new CreateQueueForm(webBrowserDriver);
        }

        /// <summary>
        /// Gets the form for creating a new queue.
        /// </summary>
        public CreateQueueForm  CreateQueueForm             { get; private set; }
    }
}