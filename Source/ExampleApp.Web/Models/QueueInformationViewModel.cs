namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Models standard queue information.
    /// </summary>
    public class QueueInformationViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueInformationViewModel"/> class.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue.</param>
        /// <param name="maxQueueSizeMegabytes">Specifies the storage capacity of the queue.</param>
        /// <param name="currentQueueSizeBytes">Specifies the current size of all the data stored in the queue.</param>
        public
        QueueInformationViewModel(
            string  queueName,
            long    maxQueueSizeMegabytes,
            long    currentQueueSizeBytes)
        {
            this.QueueName                  = queueName;
            this.MaxQueueSizeMegabytes      = maxQueueSizeMegabytes;
            this.CurrentQueueSizeMegabytes  = (currentQueueSizeBytes / 1024) / 1024;
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        public string   QueueName                   { get; private set; }

        /// <summary>
        /// Gets the storage capacity of the queue.
        /// </summary>
        public long     MaxQueueSizeMegabytes       { get; private set; }

        /// <summary>
        /// Gets the current size of all the data stored in the queue.
        /// </summary>
        public long     CurrentQueueSizeMegabytes   { get; private set; }
    }
}