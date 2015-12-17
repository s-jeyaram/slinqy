namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Specifies the required parameters to fill a queue.
    /// </summary>
    public class FillQueueCommandModel
    {
        /// <summary>
        /// Gets or sets the amount of data, in megabytes, that should be sent to the queue.
        /// </summary>
        public int SizeMegabytes { get; set; }
    }
}