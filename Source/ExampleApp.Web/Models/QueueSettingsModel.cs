namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Models changeable settings for a queue.
    /// </summary>
    public class QueueSettingsModel
    {
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="storageUtilizationScaleUpThreshold">
        /// Specifies at what point the queues storage capacity should be expanded.
        /// </param>
        public 
        QueueSettingsModel(
            double storageUtilizationScaleUpThreshold)
        {
            this.StorageUtilizationScaleUpThreshold = storageUtilizationScaleUpThreshold;
        }

        /// <summary>
        /// Gets the threshold at which point the queues storage capacity should be expanded.
        /// </summary>
        public double   StorageUtilizationScaleUpThreshold  { get; private set; }
    }
}