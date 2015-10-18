﻿namespace ExampleApp.Test.Functional.Steps
{
    using ExampleApp.Test.Functional.Models;
    using ExampleApp.Test.Functional.Models.ExampleAppPages;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Defines steps for navigating the Example App website.
    /// </summary>
    [Binding]
    public class NavigationSteps
    {
        /// <summary>
        /// Reference to the browser controller.
        /// </summary>
        private readonly WebBrowser webBrowser;

        /// <summary>
        /// Initializes a new instance with a web browser.
        /// </summary>
        /// <param name="webBrowser">Specifies the web browser to use.</param>
        public 
        NavigationSteps(
            WebBrowser webBrowser)
        {
            this.webBrowser = webBrowser;
        }

        /// <summary>
        /// Navigates to the Example App Homepage.
        /// </summary>
        [Given]
        public void GivenINavigateToTheHomepage()
        {
            // Attempt to navigate to the Home page.
            webBrowser.NavigateTo<Homepage>(); 
        }
    }
}