namespace ExampleApp.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Defines the model for sending a message to a queue.
    /// </summary>
    public class SendMessageCommandModel
    {
        /// <summary>
        /// Gets or sets the message body.
        /// </summary>
        [Required]
        [Display(Name = "Message Body")]
        public string MessageBody { get; set; }
    }
}