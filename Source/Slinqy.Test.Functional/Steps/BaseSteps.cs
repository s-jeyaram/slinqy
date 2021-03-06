﻿namespace Slinqy.Test.Functional.Steps
{
    using Models;
    using TechTalk.SpecFlow;

    /// <summary>
    /// Base class for all SpecFlow Step classes to provide some common functionality.
    /// </summary>
    public abstract class BaseSteps
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSteps"/> class.
        /// </summary>
        /// <param name="browser">
        /// Specifies the WebBrowser instance to use for interacting with the browser.
        /// </param>
        protected
        BaseSteps(
            WebBrowser browser)
        {
            this.WebBrowser = browser;
        }

        /// <summary>
        /// Gets the WebBrowser instance for interacting with the web browser.
        /// </summary>
        protected WebBrowser WebBrowser { get; }

        /// <summary>
        /// Saves the specified value in the scenario context for subsequent steps to use.
        /// </summary>
        /// <param name="value">Specifies the value to save.</param>
        /// <param name="key">Specifies a key to associate with the value.</param>
        /// <typeparam name="T">Specifies the type of the value.</typeparam>
        protected
        static
        void
        ContextSet<T>(
            T       value,
            string  key)
        {
            ScenarioContext.Current.Set(value, key);
        }

        /// <summary>
        /// Saves the specified value in the scenario context for subsequent steps to use.
        /// </summary>
        /// <param name="value">Specifies the value to save.</param>
        /// <typeparam name="T">Specifies the type of the value.</typeparam>
        protected
        static
        void
        ContextSet<T>(
            T value)
        {
            ScenarioContext.Current.Set(value);
        }

        /// <summary>
        /// Retrieves a value that was saved by a previous step.
        /// </summary>
        /// <param name="key">Specifies the key of the value to get.</param>
        /// <typeparam name="T">Specifies the type of the value.</typeparam>
        /// <returns>Returns the requested value.</returns>
        protected
        static
        T
        ContextGet<T>(
            string key)
        {
            return ScenarioContext.Current.Get<T>(key);
        }

        /// <summary>
        /// Retrieves a value that was saved by a previous step.
        /// </summary>
        /// <typeparam name="T">Specifies the type of the value.</typeparam>
        /// <returns>Returns the requested value.</returns>
        protected
        static
        T
        ContextGet<T>()
        {
            return ScenarioContext.Current.Get<T>();
        }
    }
}
