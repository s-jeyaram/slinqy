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
        private const string ValidSlinqyQueueName = "queue-name";

        private readonly IPhysicalQueueService fakePhysicalQueueService = A.Fake<IPhysicalQueueService>();

        /// <summary>
        /// Verifies that the MaxQueueSizeMegabytes property returns a sum of all the capacities of its shards.
        /// </summary>
        [Fact]
        public
        void
        MaxQueueSizeMegabytes_Always_ReturnsSumOfAllShardSizes()
        {
            // Arrange
            var fakePhysicalQueue1 = A.Fake<IPhysicalQueue>();
            var fakePhysicalQueue2 = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakePhysicalQueue1.MaxSizeMegabytes).Returns(1024);
            A.CallTo(() => fakePhysicalQueue2.MaxSizeMegabytes).Returns(1024);

            var fakeShards = new List<IPhysicalQueue> {
                fakePhysicalQueue1,
                fakePhysicalQueue2
            };

            A.CallTo(() =>
                this.fakePhysicalQueueService.ListQueues(A<string>.Ignored)
            ).Returns(fakeShards);

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
            var fakePhysicalQueue1 = A.Fake<IPhysicalQueue>();
            var fakePhysicalQueue2 = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakePhysicalQueue1.CurrentSizeBytes).Returns(1);
            A.CallTo(() => fakePhysicalQueue2.CurrentSizeBytes).Returns(2);

            var fakeShards = new List<IPhysicalQueue> {
                fakePhysicalQueue1,
                fakePhysicalQueue2
            };

            A.CallTo(() =>
                this.fakePhysicalQueueService.ListQueues(A<string>.Ignored)
            ).Returns(fakeShards);

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
            var fakeReadablePhysicalQueue = A.Fake<IPhysicalQueue>();
            var fakeWritablePhysicalQueue = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakeReadablePhysicalQueue.Writable).Returns(false);
            A.CallTo(() => fakeWritablePhysicalQueue.Writable).Returns(true);

            var fakeShards = new List<IPhysicalQueue> {
                fakeReadablePhysicalQueue,
                fakeWritablePhysicalQueue
            };

            A.CallTo(() =>
                this.fakePhysicalQueueService.ListQueues(A<string>.Ignored)
            ).Returns(fakeShards);

            var slinqyQueue = new SlinqyQueue(
                ValidSlinqyQueueName,
                this.fakePhysicalQueueService
            );

            var validBatch = new List<string> { "message 1", "message 2" };

            // Act
            await slinqyQueue.SendBatch(validBatch);

            // Assert
            A.CallTo(() =>
                fakeWritablePhysicalQueue.SendBatch(A<IEnumerable<object>>.Ignored)
            ).MustHaveHappened();
        }
    }
}
