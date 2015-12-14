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
        /// The fake that simulates a queue shard monitor.
        /// </summary>
        private readonly SlinqyQueueShardMonitor fakeQueueShardMonitor = A.Fake<SlinqyQueueShardMonitor>();

        /// <summary>
        /// The fake that simulates a read-only queue shard.
        /// </summary>
        private readonly SlinqyQueueShard fakeReadShard;

        /// <summary>
        /// The fake that simulates a writable queue shard.
        /// </summary>
        private readonly SlinqyQueueShard fakeWriteShard;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueTests"/> class.
        /// </summary>
        public
        SlinqyQueueTests()
        {
            this.fakeReadShard  = A.Fake<SlinqyQueueShard>();
            this.fakeWriteShard = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => this.fakeReadShard.PhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => this.fakeWriteShard.PhysicalQueue.IsSendEnabled).Returns(true);

            var shards = new List<SlinqyQueueShard> {
                this.fakeReadShard,
                this.fakeWriteShard
            };

            A.CallTo(() => this.fakeQueueShardMonitor.Shards).Returns(shards);
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
            A.CallTo(() => this.fakeReadShard.PhysicalQueue.MaxSizeMegabytes).Returns(1024);
            A.CallTo(() => this.fakeWriteShard.PhysicalQueue.MaxSizeMegabytes).Returns(1024);

            var slinqyQueue = new SlinqyQueue(
                this.fakeQueueShardMonitor
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
            A.CallTo(() => this.fakeReadShard.PhysicalQueue.CurrentSizeBytes).Returns(1);
            A.CallTo(() => this.fakeWriteShard.PhysicalQueue.CurrentSizeBytes).Returns(2);

            var slinqyQueue = new SlinqyQueue(
                this.fakeQueueShardMonitor
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
                this.fakeQueueShardMonitor
            );

            var validBatch = new List<string> { "message 1", "message 2" };

            // Act
            await slinqyQueue.SendBatch(validBatch);

            // Assert
            A.CallTo(() =>
                this.fakeWriteShard.SendBatch(A<IEnumerable<object>>.Ignored)
            ).MustHaveHappened();
        }
    }
}