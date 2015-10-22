namespace Slinqy.Test.Functional.Steps
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Models;
    using Models.ExampleAppPages;
    using System.Reflection;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Defines steps for using the versioning infrastructure.
    /// </summary>
    [Binding]
    public class VersionSteps
    {
        /// <summary>
        /// Reference to the browser controller.
        /// </summary>
        private readonly WebBrowser webBrowser;

        /// <summary>
        /// Initializes a new instance with a web browser.
        /// </summary>
        /// <param name="webBrowser">Specifies the browser.</param>
        public 
        VersionSteps(
            WebBrowser webBrowser)
        {
            this.webBrowser = webBrowser;
        }

        /// <summary>
        /// Verifies that the version number displayed on the web page 
        /// footer matches the version number of the local test assembly 
        /// ensuring they were both built from the same source code.
        /// </summary>
        [Then]
        public 
        void 
        ThenTheApplicationVersionMatchesTheTestVersion()
        {
            var examplePage = this.webBrowser.GetCurrentPageAs<SlinqyExampleWebpage>();

            var websiteVersion = examplePage.Footer.Version;
            var testVersion    = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var errorMessage = @"
The version of test code does not match the version of the deployed code, which could be because:
A) The latest code has not been deployed,
B) The previous deployment of the latest code failed, or
C) There is a bug related to versioning.";

            Assert.AreEqual(
                testVersion, 
                websiteVersion, 
                errorMessage
            );
        }
    }
}