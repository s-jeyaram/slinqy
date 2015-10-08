using TechTalk.SpecFlow;

namespace ExampleApp.Test.Functional.Steps
{
    [Binding]
    public class NavigationSteps
    {
        [Given(@"I navigate to the Home page")]
        public void GivenINavigateToTheHomePage()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
