namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Models standard queue information.
    /// </summary>
    public class QueueInformation
    {
        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        public string QueueName { get; private set; }

        /// <summary>
        /// Gets the storage capacity, in gigabytes, of the queue.
        /// </summary>
        public long StorageCapacityMegabytes { get; private set; }

        /// <summary>
        /// Initializes the instance with information about the queue.
        /// </summary>
        /// <param name="queueName">Specifies the name of the queue.</param>
        /// <param name="storageCapacityMegabytesMegabytes">Specifies the storage capacity of the queue.</param>
        public 
        QueueInformation(
            string queueName,
            long storageCapacityMegabytesMegabytes)
        {
            this.QueueName                = queueName;
            this.StorageCapacityMegabytes = storageCapacityMegabytesMegabytes;
        }
    }
}