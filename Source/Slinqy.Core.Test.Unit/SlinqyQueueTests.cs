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

            A.CallTo(() => this.fakeReadShard.Writable).Returns(false);
            A.CallTo(() => this.fakeWriteShard.Writable).Returns(true);

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
            A.CallTo(() => this.fakeReadShard.MaxSizeMegabytes).Returns(1024);
            A.CallTo(() => this.fakeWriteShard.MaxSizeMegabytes).Returns(1024);

            var slinqyQueue = new SlinqyQueue(
                ValidSlinqyQueueName,
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
            A.CallTo(() => this.fakeReadShard.CurrentSizeBytes).Returns(1);
            A.CallTo(() => this.fakeWriteShard.CurrentSizeBytes).Returns(2);

            var slinqyQueue = new SlinqyQueue(
                ValidSlinqyQueueName,
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
                ValidSlinqyQueueName,
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