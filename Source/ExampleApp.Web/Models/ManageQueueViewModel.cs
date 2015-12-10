namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Models an existing queue.
    /// </summary>
    public class ManageQueueViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManageQueueViewModel"/> class.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue this instance represents.</param>
        /// <param name="storageCapacityMegabytes">Specifies the storage capacity of the queue.</param>
        /// <param name="currentQueueSizeBytes">Specifies the current size of all the data stored in the queue.</param>
        public
        ManageQueueViewModel(
            string  queueName,
            long    storageCapacityMegabytes,
            long    currentQueueSizeBytes)
        {
            this.QueueSettings    = new QueueSettingsViewModel(0.5);
            this.QueueInformation = new QueueInformationViewModel(
                queueName,
                storageCapacityMegabytes,
                currentQueueSizeBytes
            );
        }

        /// <summary>
        /// Gets information about the queue.
        /// </summary>
        public QueueInformationViewModel QueueInformation { get; private set; }

        /// <summary>
        /// Gets the current settings for the queue.
        /// </summary>
        public QueueSettingsViewModel    QueueSettings    { get; private set; }
    }
}