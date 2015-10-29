namespace Slinqy.Core
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Models a virtual queue.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Queue is not a suffix in this case.")]
    public class SlinqyQueue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueue"/> class.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="maxSizeInMegabytes">Specifies the storage capacity of the queue.</param>
        public
        SlinqyQueue(
            string  queueName,
            long    maxSizeInMegabytes)
        {
            this.Name                   = queueName;
            this.MaximumSizeInMegabytes = maxSizeInMegabytes;
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        public string   Name                    { get; private set; }

        /// <summary>
        /// Gets the maximum storage capacity of the queue.
        /// </summary>
        public long     MaximumSizeInMegabytes  { get; private set; }
    }
}
