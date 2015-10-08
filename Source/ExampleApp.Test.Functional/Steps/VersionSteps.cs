using TechTalk.SpecFlow;

namespace ExampleApp.Test.Functional.Steps
{
    [Binding]
    public class VersionSteps
    {
        [Then(@"the Application Version matches the Test Version")]
        public void ThenTheApplicationVersionMatchesTheTestVersion()
        {
            ScenarioContext.Current.Pending();
        }
    }
}
