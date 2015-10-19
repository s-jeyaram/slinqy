namespace ExampleApp.Test.Functional
{
    using BoDi;
    using Models;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using System;
    using System.Configuration;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Code for setting up the local environment for performing the tests.
    /// </summary>
    [Binding]
    public sealed class Setup : IDisposable
    {
        /// <summary>
        /// Simple object container for SpecFlows dependency injection of objects in to Step classes.
        /// </summary>
        private readonly IObjectContainer objectContainer;

        /// <summary>
        /// Maintains a reference to the current web browser.
        /// </summary>
        private WebBrowser webBrowser;
        
        /// <summary>
        /// Maintains a reference to the current web driver.
        /// </summary>
        private IWebDriver webDriver;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="objectContainer">Specifies the object container instance to use for DI.</param>
        public
        Setup(
            IObjectContainer objectContainer)
        {
            this.objectContainer = objectContainer;
        }

        /// <summary>
        /// Initializes the web browser for UI tests.
        /// </summary>
        [BeforeScenario]
        public
        void 
        InitializeWebBrowser()
        {
            this.webDriver        = new ChromeDriver();
            var exampleAppBaseUri = new Uri(GetSetting("ExampleApp.BaseUri"));
                                  
            this.webBrowser       = new WebBrowser(this.webDriver, exampleAppBaseUri);

            this.objectContainer.RegisterInstanceAs(this.webBrowser);
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

        /// <summary>
        /// Disposes resources.
        /// </summary>
        public 
        void 
        Dispose()
        {
            this.webBrowser.Dispose();
            this.webDriver.Dispose();
        }
    }
}