namespace ExampleApp.Web.Models
{
    /// <summary>
    /// Models changeable settings for a queue.
    /// </summary>
    public class QueueSettingsViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueueSettingsViewModel"/> class.
        /// </summary>
        /// <param name="storageUtilizationScaleUpThreshold">
        /// Specifies at what point the queues storage capacity should be expanded.
        /// </param>
        public
        QueueSettingsViewModel(
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