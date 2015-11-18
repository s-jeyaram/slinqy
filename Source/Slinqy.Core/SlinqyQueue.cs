namespace Slinqy.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Models a virtual queue that is made up of n number of physical shards.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Queue is not a suffix in this case.")]
    public sealed class SlinqyQueue : IDisposable
    {
        /// <summary>
        /// Delegate to the function for listing physical queues.
        /// </summary>
        private readonly Func<string, Task<IEnumerable<SlinqyQueueShard>>> listPhysicalQueuesDelegate;

        /// <summary>
        /// The async Task that is updating the list of physical queue shards.
        /// </summary>
        private readonly Task pollShardsTask;

        /// <summary>
        /// The cancellation token that can be used to cancel the pollShardsTask.
        /// </summary>
        private readonly CancellationTokenSource pollShardsTaskCancellationToken;

        /// <summary>
        /// Maintains the collection of physical shards that make up this virtual queue.
        /// </summary>
        private IEnumerable<SlinqyQueueShard> shards = Enumerable.Empty<SlinqyQueueShard>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueue"/> class.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="listPhysicalQueuesDelegate">Specifies the delegate for listing physical queues.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This rule was not designed for async calls.")]
        public
        SlinqyQueue(
            string                                              queueName,
            Func<string, Task<IEnumerable<SlinqyQueueShard>>>   listPhysicalQueuesDelegate)
        {
            this.listPhysicalQueuesDelegate = listPhysicalQueuesDelegate;

            this.Name = queueName;

            this.pollShardsTaskCancellationToken = new CancellationTokenSource();
            this.pollShardsTask = this.PollShards(this.pollShardsTaskCancellationToken.Token);
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the current maximum storage capacity of the virtual queue.
        /// </summary>
        public long MaxQueueSizeMegabytes
        {
            get
            {
                return this.shards.Sum(s => s.MaxSizeInMegabytes);
            }
        }

        /// <summary>
        /// Disposes of any resources created during the life time of this instance.
        /// </summary>
        public
        void
        Dispose()
        {
            // Cancel polling.
            if (!this.pollShardsTaskCancellationToken.IsCancellationRequested)
                this.pollShardsTaskCancellationToken.Cancel();

            // Wait for the task to finish.
            while (!this.pollShardsTask.IsCompleted)
                Thread.Sleep(100);

            // Make sure all resources are disposed.
            this.pollShardsTask.Dispose();
            this.pollShardsTaskCancellationToken.Dispose();
        }

        /// <summary>
        /// Retrieves the current list of physical shards and updates the internal list.
        /// </summary>
        /// <param name="cancellationToken">
        /// Specifies a token that can be used to cancel async operations.
        /// </param>
        /// <returns>Returns the Task of the asyncronous operation.</returns>
        private
        async Task
        PollShards(
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) {
                await this.PopulateShards().ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }

        /// <summary>
        /// Gets the latest list of physical queues from the physical resource.
        /// </summary>
        /// <returns>Returns the asynchronous Task.</returns>
        private
        async Task
        PopulateShards()
        {
            // Create and start the async task for listing queues.
            var listTask = this.listPhysicalQueuesDelegate(this.Name);

            // Wait for the task to complete...
            // Disable need to return to the original context thread, this prevents deadlocks if being hosted within ASP.NET.
            // For more info:
            // - http://stackoverflow.com/questions/13489065/best-practice-to-call-configureawait-for-all-server-side-code
            // - http://www.tugberkugurlu.com/archive/asynchronousnet-client-libraries-for-your-http-api-and-awareness-of-async-await-s-bad-effects
            await listTask.ConfigureAwait(false);
        }
    }
}
