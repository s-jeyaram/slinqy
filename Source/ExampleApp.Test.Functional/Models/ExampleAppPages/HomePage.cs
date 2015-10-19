namespace ExampleApp.Test.Functional.Models.ExampleAppPages
{
    using OpenQA.Selenium;
    using System;
    using System.Collections.Generic;
    using System.Linq;


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
        /// Gets or sets the threshold for when the queue agent should scale the queue storage capacity.
        /// </summary>
        public double                       ScaleUpThresholdPercentage  { get; set; }

        /// <summary>
        /// Gets the history of the queue.
        /// </summary>
        public IEnumerable<QueueHistory>    QueueHistory
        {
            get
            {
                this.ToString();
                return Enumerable.Empty<QueueHistory>();
            }
        }

        /// <summary>
        /// Initializes the Homepage with the IWebDriver to use for controlling the page.
        /// </summary>
        /// <param name="webBrowserDriver">
        /// Specifies the IWebDriver to use to interact with the Homepage.
        /// </param>
        public 
        Homepage(
            IWebDriver webBrowserDriver) 
                : base(webBrowserDriver, new Uri(RelativePath, UriKind.Relative))
        {
        }

        /// <summary>
        /// Applies the settings to the agent.
        /// </summary>
        public 
        void 
        UpdateAgent()
        {
            this.ToString();
        }

        /// <summary>
        /// Generates queue messages and submits them to the queue.
        /// </summary>
        public void GenerateQueueMessages()
        {
            this.ToString();
        }
    }
}