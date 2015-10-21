namespace ExampleApp.Models
{
    /// <summary>
    /// Models an existing queue.
    /// </summary>
    public class ManageQueueModel
    {
        /// <summary>
        /// Gets information about the queue.
        /// </summary>
        public QueueInformation QueueInformation { get; private set; }

        /// <summary>
        /// Gets the current settings for the queue.
        /// </summary>
        public QueueSettings    QueueSettings    { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue this instance represents.</param>
        /// <param name="storageCapacityMegabytes">Specifies the storage capacity of the queue.</param>
        public 
        ManageQueueModel(
            string  queueName,
            long     storageCapacityMegabytes)
        {
            this.QueueSettings    = new QueueSettings(0.5);
            this.QueueInformation = new QueueInformation(queueName, storageCapacityMegabytes);
        }
    }
}