using System;
using ExampleApp.Test.Functional.Models;
using TechTalk.SpecFlow;

namespace ExampleApp.Test.Functional.Steps
{
    [Binding]
    public class NavigationSteps
    {
        private readonly WebBrowser _webBrowser;

        public 
        NavigationSteps(
            WebBrowser webBrowser)
        {
            _webBrowser = webBrowser;
        }

        [Given]
        public void GivenINavigateToTheHomePage()
        {
            // Attempt to navigate to the Home page.
            _webBrowser.NavigateTo<HomePage>(
                baseUri: new Uri("http://localhost:17057") // TODO: Make configurable.
            ); 
        }
    }
}