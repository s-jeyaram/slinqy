namespace ExampleApp.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines the model for creating a new queue.
    /// </summary>
    public class CreateQueueCommandModel
    {
        /// <summary>
        /// Gets or sets the name of the queue to create.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Queue Name")]
        public string QueueName { get; set; }

        /// <summary>
        /// Gets or sets the max storage capacity for the queue.
        /// </summary>
        [Required]
        [Display(Name = "Max Size (megabytes)")]
        public long MaxQueueSizeMegabytes { get; set; }

        /// <summary>
        /// Gets or sets the percentage at which the agent will scale out the virtual SlinqyQueue.
        /// </summary>
        [Required]
        [Range(1, 100)]
        [Display(Name = "Storage Capacity ScaleOut Threshold")]
        public int StorageCapacityScaleOutThresholdPercentage { get; set; }
    }
}