namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Defines the model for fill queue status information.
    /// </summary>
    public class FillQueueStatusViewModel
    {
        /// <summary>
        /// Gets or sets a value that indicates the current status of the queues fill operation.
        /// </summary>
        public FillQueueStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the number of messages that have been sent since the fill request started.
        /// </summary>
        public int SentCount { get; set; }
    }
}