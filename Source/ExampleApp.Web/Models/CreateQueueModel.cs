namespace ExampleApp.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines the model for creating a new queue.
    /// </summary>
    public class CreateQueueModel
    {
        /// <summary>
        /// Gets or sets the name of the queue to create.
        /// </summary>
        [Required]
        [Display(Name = "Queue Name")]
        public string QueueName { get; set; }

        /// <summary>
        /// Gets or sets the max storage capacity for the queue.
        /// </summary>
        [Required]
        [Display(Name = "Max Size (megabytes)")]
        public long MaxQueueSizeMegabytes { get; set; }
    }
}