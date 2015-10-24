namespace Slinqy.Test.Functional.Steps
{
    using Models;
    using Models.ExampleAppPages;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Defines steps for navigating the Example App website.
    /// </summary>
    [Binding]
    public class NavigationSteps : BaseSteps
    {
        /// <summary>
        /// Initializes a new instance with a web browser.
        /// </summary>
        /// <param name="webBrowser">Specifies the web browser to use.</param>
        public 
        NavigationSteps(
            WebBrowser webBrowser) : base(webBrowser)
        {
        }

        /// <summary>
        /// Navigates to the Example App Homepage.
        /// </summary>
        [Given]
        public 
        void 
        GivenINavigateToTheHomepage()
        {
            // Attempt to navigate to the Home page.
            this.WebBrowser.NavigateTo<Homepage>(); 
        }
    }
}