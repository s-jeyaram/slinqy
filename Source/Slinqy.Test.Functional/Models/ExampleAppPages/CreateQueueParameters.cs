namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    /// <summary>
    /// Models the parameters for creating a new Slinqy queue on the Example App Homepage.
    /// </summary>
    public class CreateQueueParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQueueParameters"/> class.
        /// </summary>
        /// <param name="queueName">Specifies the name of the new queue.</param>
        /// <param name="storageCapacityMegabytes">Specifies the desired storage capacity for the new queue.</param>
        /// <param name="scaleUpThresholdPercentage">Specifies the scale up threshold for the new queue.</param>
        /// <param name="randomizeQueueName">Specifies if random characters should be added to the queueName.</param>
        public CreateQueueParameters(
            string  queueName,
            int     storageCapacityMegabytes,
            int     scaleUpThresholdPercentage,
            bool    randomizeQueueName)
        {
            this.QueueName                  = queueName;
            this.RandomizeQueueName         = randomizeQueueName;
            this.StorageCapacityMegabytes   = storageCapacityMegabytes;
            this.ScaleUpThresholdPercentage = scaleUpThresholdPercentage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQueueParameters"/> class.
        /// </summary>
        /// <param name="queueName">Specifies the name of the new queue.</param>
        /// <param name="storageCapacityMegabytes">Specifies the desired storage capacity for the new queue.</param>
        /// <param name="scaleUpThresholdPercentage">Specifies the scale up threshold for the new queue.</param>
        public CreateQueueParameters(
            string  queueName,
            int     storageCapacityMegabytes,
            int     scaleUpThresholdPercentage)
                : this(queueName, storageCapacityMegabytes, scaleUpThresholdPercentage, true)
        {
        }

        /// <summary>
        /// Gets a value indicating whether random characters will be appended (true) to the QueueName, or not (false).
        /// </summary>
        public bool     RandomizeQueueName          { get; private set; }

        /// <summary>
        /// Gets the name of the new queue.
        /// </summary>
        public string   QueueName                   { get; private set; }

        /// <summary>
        /// Gets the desired storage capacity for the new queue.
        /// </summary>
        public long     StorageCapacityMegabytes    { get; private set; }

        /// <summary>
        /// Gets the scale up threshold setting.
        /// </summary>
        public int      ScaleUpThresholdPercentage  { get; private set; }
    }
}
