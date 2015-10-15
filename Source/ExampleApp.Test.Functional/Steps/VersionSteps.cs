using ExampleApp.Test.Functional.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using TechTalk.SpecFlow;

namespace ExampleApp.Test.Functional.Steps
{
    [Binding]
    public class VersionSteps
    {
        private WebBrowser _webBrowser;

        public VersionSteps(WebBrowser webBrowser)
        {
            _webBrowser = webBrowser;
        }

        [Then]
        public void ThenTheApplicationVersionMatchesTheTestVersion()
        {
            var examplePage = _webBrowser.GetCurrentPageAs<SlinqyExampleWebPage>();

            var websiteVersion = examplePage.Footer.Version;
            var testVersion    = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Assert.AreEqual(
                testVersion, 
                websiteVersion, 
                @"
The version of test code does not match the version of the deployed code, which could be because:
A) The latest code has not been deployed,
B) The previous deployment of the latest code failed, or
C) There is a bug related to versioning."
            );
        }
    }
}