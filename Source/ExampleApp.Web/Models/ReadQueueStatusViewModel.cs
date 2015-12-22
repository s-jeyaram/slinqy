namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Defines the model for read queue status information.
    /// </summary>
    public class ReadQueueStatusViewModel
    {
        /// <summary>
        /// Gets or sets a value that indicates the current status of the queues read operation.
        /// </summary>
        public ReadQueueStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the number of messages that have been sent since the read request started.
        /// </summary>
        public int ReceivedCount { get; set; }
    }
}