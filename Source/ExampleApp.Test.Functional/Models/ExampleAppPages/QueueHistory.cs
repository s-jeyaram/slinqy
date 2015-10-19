namespace ExampleApp.Test.Functional.Models.ExampleAppPages
{
    using System;

    /// <summary>
    /// Models a record of queue history.
    /// </summary>
    public class QueueHistory
    {
        /// <summary>
        /// Gets the time stamp when the history information was reported.
        /// </summary>
        public DateTimeOffset Timestamp { get; private set; }

        /// <summary>
        /// Gets the storage capacity in gigabytes.
        /// </summary>
        public int StorageCapacityGigabytes { get; private set; }

        /// <summary>
        /// Initializes the queue history record.
        /// </summary>
        public 
        QueueHistory()
        {
            this.Timestamp                = DateTimeOffset.MaxValue;
            this.StorageCapacityGigabytes = Int32.MaxValue;
        }
    }
}
