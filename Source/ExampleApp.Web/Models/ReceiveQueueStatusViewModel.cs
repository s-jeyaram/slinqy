namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Defines the model for receive queue status information.
    /// </summary>
    public class ReceiveQueueStatusViewModel
    {
        /// <summary>
        /// Gets or sets a value that indicates the current status of the queues receive operation.
        /// </summary>
        public ReceiveQueueStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the number of messages that have been sent since the receive request started.
        /// </summary>
        public int ReceivedCount { get; set; }
    }
}