namespace Slinqy.Core.Test.Unit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    /// <summary>
    /// Tests functions of the SlinqyQueue class.
    /// </summary>
    public class SlinqyQueueTests
    {
        /// <summary>
        /// Represents a valid value where one is needed for a Slinqy queue name parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const string ValidSlinqyQueueName = "queue-name";

        /// <summary>
        /// The fake that simulates a physical queue service.
        /// </summary>
        private readonly IPhysicalQueueService fakePhysicalQueueService = A.Fake<IPhysicalQueueService>();

        /// <summary>
        /// The fake that simulates a read-only physical queue.
        /// </summary>
        private readonly IPhysicalQueue fakeReadOnlyPhysicalQueue = A.Fake<IPhysicalQueue>();

        /// <summary>
        /// The fake that simulates a writable physical queue.
        /// </summary>
        private readonly IPhysicalQueue fakeWritablePhysicalQueue = A.Fake<IPhysicalQueue>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueTests"/> class.
        /// </summary>
        public
        SlinqyQueueTests()
        {
            var fakeShards = new List<IPhysicalQueue> {
                this.fakeReadOnlyPhysicalQueue,
                this.fakeWritablePhysicalQueue
            };

            A.CallTo(() => this.fakeReadOnlyPhysicalQueue.Name).Returns(ValidSlinqyQueueName + "0");
            A.CallTo(() => this.fakeWritablePhysicalQueue.Name).Returns(ValidSlinqyQueueName + "1");

            A.CallTo(() => this.fakeReadOnlyPhysicalQueue.Writable).Returns(false);
            A.CallTo(() => this.fakeWritablePhysicalQueue.Writable).Returns(true);

            A.CallTo(() =>
                this.fakePhysicalQueueService.ListQueues(A<string>.Ignored)
            ).Returns(fakeShards);
        }

        /// <summary>
        /// Verifies that the MaxQueueSizeMegabytes property returns a sum of all the capacities of its shards.
        /// </summary>
        [Fact]
        public
        void
        MaxQueueSizeMegabytes_Always_ReturnsSumOfAllShardSizes()
        {
            // Arrange
            A.CallTo(() => this.fakeReadOnlyPhysicalQueue.MaxSizeMegabytes).Returns(1024);
            A.CallTo(() => this.fakeWritablePhysicalQueue.MaxSizeMegabytes).Returns(1024);

            var slinqyQueue = new SlinqyQueue(
                ValidSlinqyQueueName,
                this.fakePhysicalQueueService
            );

            // Act
            var actual = slinqyQueue.MaxQueueSizeMegabytes;

            // Assert
            Assert.Equal(2048, actual);
        }

        /// <summary>
        /// Verifies that the CurrentQueueSizeBytes property returns a sum of all the current sizes of its shards.
        /// </summary>
        [Fact]
        public
        void
        CurrentQueueSizeBytes_Always_ReturnsSumOfAllShardSizes()
        {
            // Arrange
            A.CallTo(() => this.fakeReadOnlyPhysicalQueue.CurrentSizeBytes).Returns(1);
            A.CallTo(() => this.fakeWritablePhysicalQueue.CurrentSizeBytes).Returns(2);

            var slinqyQueue = new SlinqyQueue(
                ValidSlinqyQueueName,
                this.fakePhysicalQueueService
            );

            // Act
            var actual = slinqyQueue.CurrentQueueSizeBytes;

            // Assert
            Assert.Equal(3, actual);
        }

        /// <summary>
        /// Verifies that the SendBatch method properly submits the batch to the underlying write shard.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public
        async Task
        SendBatch_BatchIsValid_BatchSentToWriteShard()
        {
            // Arrange
            var slinqyQueue = new SlinqyQueue(
                ValidSlinqyQueueName,
                this.fakePhysicalQueueService
            );

            var validBatch = new List<string> { "message 1", "message 2" };

            // Act
            await slinqyQueue.SendBatch(validBatch);

            // Assert
            A.CallTo(() =>
                this.fakeWritablePhysicalQueue.SendBatch(A<IEnumerable<object>>.Ignored)
            ).MustHaveHappened();
        }
    }
}
