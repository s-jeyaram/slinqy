using System;
using System.Configuration;
using BoDi;
using ExampleApp.Test.Functional.Models;
using TechTalk.SpecFlow;

namespace ExampleApp.Test.Functional
{
    /// <summary>
    /// Code for setting up the local environment for performing the tests.
    /// </summary>
    [Binding]
    public class Setup
    {
        /// <summary>
        /// Simple object container for SpecFlows dependency injection of objects in to Step classes.
        /// </summary>
        private readonly IObjectContainer _objectContainer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="objectContainer">Specifies the object container instance to use for DI.</param>
        public
        Setup(
            IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        /// <summary>
        /// Initializes the web browser for UI tests.
        /// </summary>
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

        /// <summary>
        /// Gets the specified configuration setting.
        /// </summary>
        /// <param name="settingName">Specifies the name of the configuration setting to get.</param>
        /// <returns>Returns the value of the configuration setting if found.  Otherwise null is returned.</returns>
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