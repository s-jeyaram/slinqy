using System;
using System.Configuration;
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
        public
        void 
        InitializeWebBrowser()
        {
            // Get the base url from configuration
            var exampleAppBaseUri = new Uri(GetSetting("ExampleApp.BaseUri"));
            var webBrowser        = new WebBrowser(exampleAppBaseUri);

            _objectContainer.RegisterInstanceAs(webBrowser);
        }

        private 
        static 
        string 
        GetSetting(
            string settingName)
        {
            var settingValue = Environment.GetEnvironmentVariable(settingName);

            if (string.IsNullOrWhiteSpace(settingValue))
                settingValue = ConfigurationManager.AppSettings[settingName];

            return settingValue;
        }
    }
}