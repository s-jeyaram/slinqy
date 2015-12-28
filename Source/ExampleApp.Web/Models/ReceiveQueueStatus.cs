namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Defines the possible states of a receive queue operation.
    /// </summary>
    public enum ReceiveQueueStatus
    {
        /// <summary>
        /// Indicates the variable has not been initialized with a valid value.
        /// </summary>
        Uninitialized,

        /// <summary>
        /// Indicates that the receive queue operation is currently occurring.
        /// </summary>
        Running,

        /// <summary>
        /// Indicates that the receive queue operation has completed.
        /// </summary>
        Finished
    }
}