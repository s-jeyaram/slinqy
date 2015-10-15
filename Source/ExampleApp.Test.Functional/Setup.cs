using BoDi;
using ExampleApp.Test.Functional.Models;
using TechTalk.SpecFlow;

namespace ExampleApp.Test.Functional
{
    [Binding]
    public class Setup
    {
        private readonly IObjectContainer _objectContainer;

        public 
        Setup(
            IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeScenario]
        public void InitializeWebBrowser()
        {
            var webBrowser = new WebBrowser();

            _objectContainer.RegisterInstanceAs(webBrowser);
        }
    }
}