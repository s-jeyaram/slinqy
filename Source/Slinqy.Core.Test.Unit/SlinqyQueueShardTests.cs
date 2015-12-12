namespace Slinqy.Core.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    /// <summary>
    /// Tests functionality of the SlinqyQueueShard class.
    /// </summary>
    public class SlinqyQueueShardTests
    {
        /// <summary>
        /// Represents a valid value where one is needed for a Slinqy shard name parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const string ValidSlinqyShardName = "queue-name-0";

        /// <summary>
        /// The fake that simulates a physical queue.
        /// </summary>
        private readonly IPhysicalQueue fakePhysicalQueue = A.Fake<IPhysicalQueue>();

        /// <summary>
        /// The SlinqyQueueShard instance under test.
        /// </summary>
        private readonly SlinqyQueueShard slinqyQueueShard;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueShardTests"/> class.
        /// </summary>
        public
        SlinqyQueueShardTests()
        {
            A.CallTo(() => this.fakePhysicalQueue.Name).Returns(ValidSlinqyShardName);

            this.slinqyQueueShard = new SlinqyQueueShard(this.fakePhysicalQueue);
        }

        /// <summary>
        /// Verifies that the constructor checks the physicalQueue parameter.
        /// </summary>
        [Fact]
        public
        static
        void
        Constructor_PhysicalQueueParameterIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SlinqyQueueShard(null));
        }

        /// <summary>
        /// Verifies that the SendBatch method properly submits the batch to the underlying physical queue.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public
        async Task
        SendBatch_BatchIsValid_BatchSentToPhysicalQueue()
        {
            // Arrange
            var validBatch = new List<string> { "message 1", "message 2" };

            // Act
            await this.slinqyQueueShard.SendBatch(validBatch);

            // Assert
            A.CallTo(() =>
                this.fakePhysicalQueue.SendBatch(A<IEnumerable<object>>.Ignored)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the ShardIndex property returns the shards index based on the physical shards name.
        /// </summary>
        [Fact]
        public
        void
        ShardIndex_Always_ReturnsIndexFromName()
        {
            // Arrange
            A.CallTo(() =>
                this.fakePhysicalQueue.Name
            ).Returns("queue-100");

            // Act
            var actualShardIndex = this.slinqyQueueShard.ShardIndex;

            // Assert
            Assert.Equal(100, actualShardIndex);
        }

        /// <summary>
        /// Verifies that the StorageUtilization property correctly returns the
        /// storage utilization percentage based on the underlying physical queue.
        /// </summary>
        [Fact]
        public
        void
        StorageUtilization_PhysicalQueuePartiallyFull_ReturnsPercentage()
        {
            // Arrange
            A.CallTo(() => this.fakePhysicalQueue.MaxSizeMegabytes).Returns(1);
            A.CallTo(() => this.fakePhysicalQueue.CurrentSizeBytes).Returns(524288);

            // Act
            var actualPercentage = this.slinqyQueueShard.StorageUtilization;

            // Assert
            Assert.Equal(0.5, actualPercentage);
        }
    }
}
