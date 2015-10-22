namespace Slinqy.Test.Functional.Models.ExampleAppPages
{
    /// <summary>
    /// Models the parameters for creating a new Slinqy queue on the Example App Homepage.
    /// </summary>
    public class CreateQueueParameters
    {
        /// <summary>
        /// Initializes a new instance with the desired settings for the new queue.
        /// </summary>
        /// <param name="queueName">Specifies the name of the new queue.</param>
        /// <param name="storageCapacityMegabytes">Specifies the desired storage capacity for the new queue.</param>
        /// <param name="scaleUpThresholdPercentage">Specifies the scale up threshold for the new queue.</param>
        public CreateQueueParameters(
            string  queueName,
            int     storageCapacityMegabytes,
            double  scaleUpThresholdPercentage)
        {
            this.QueueName                  = queueName;
            this.StorageCapacityMegabytes   = storageCapacityMegabytes;
            this.ScaleUpThresholdPercentage = scaleUpThresholdPercentage;
        }

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
        public double   ScaleUpThresholdPercentage  { get; private set; }
    }
}
