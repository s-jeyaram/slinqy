namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Defines the possible states of a fill queue operation.
    /// </summary>
    public enum FillQueueStatus
    {
        /// <summary>
        /// Indicates the variable has not been initialized with a valid value.
        /// </summary>
        Uninitialized,

        /// <summary>
        /// Indicates that the fill queue operation is currently occurring.
        /// </summary>
        Running,

        /// <summary>
        /// Indicates that the fill queue operation has completed.
        /// </summary>
        Finished
    }
}