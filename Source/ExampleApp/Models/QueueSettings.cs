namespace ExampleApp.Models
{
    /// <summary>
    /// Models changeable settings for a queue.
    /// </summary>
    public class QueueSettings
    {
        /// <summary>
        /// Gets the threshold at which point the queues storage capacity should be expanded.
        /// </summary>
        public double StorageUtilizationScaleUpThreshold { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="storageUtilizationScaleUpThreshold">
        /// Specifies at what point the queues storage capacity should be expanded.
        /// </param>
        public 
        QueueSettings(
            double storageUtilizationScaleUpThreshold)
        {
            this.StorageUtilizationScaleUpThreshold = storageUtilizationScaleUpThreshold;
        }
    }
}