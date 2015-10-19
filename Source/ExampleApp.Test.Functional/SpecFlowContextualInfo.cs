namespace ExampleApp.Test.Functional
{
    using System;

    /// <summary>
    /// Maintains scenario context information.
    /// </summary>
    public class SpecFlowContextualInfo
    {
        /// <summary>
        /// Gets the time stamp when the scenario started.
        /// </summary>
        public DateTimeOffset ScenarioStartTimestamp { get; private set; }

        /// <summary>
        /// Instantiates a new instance with time stamp of when the scenario started.
        /// </summary>
        /// <param name="scenarioStartTimestamp">
        /// Specifies the full date and time when the scenario first started executing.
        /// </param>
        public 
        SpecFlowContextualInfo(
            DateTimeOffset scenarioStartTimestamp)
        {
            this.ScenarioStartTimestamp = scenarioStartTimestamp;
        }
    }
}
