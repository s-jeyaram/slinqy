namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Defines the possible states of a read queue operation.
    /// </summary>
    public enum ReadQueueStatus
    {
        /// <summary>
        /// Indicates the variable has not been initialized with a valid value.
        /// </summary>
        Uninitialized,

        /// <summary>
        /// Indicates that the read queue operation is currently occurring.
        /// </summary>
        Running,

        /// <summary>
        /// Indicates that the read queue operation has completed.
        /// </summary>
        Finished
    }
}