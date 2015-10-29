namespace Slinqy.Test.Functional
{
    using System;

    /// <summary>
    /// Maintains scenario context information.
    /// </summary>
    public class SpecFlowContextualInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpecFlowContextualInfo"/> class.
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

        /// <summary>
        /// Gets the time stamp when the scenario started.
        /// </summary>
        public DateTimeOffset   ScenarioStartTimestamp  { get; private set; }
    }
}
