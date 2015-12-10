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
        private const string ValidSlinqyShardName = "queue-name-0";

        private readonly IPhysicalQueue fakePhysicalQueue = A.Fake<IPhysicalQueue>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueShardTests"/> class.
        /// </summary>
        public
        SlinqyQueueShardTests()
        {
            A.CallTo(() => this.fakePhysicalQueue.Name).Returns(ValidSlinqyShardName);
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
            var queueShard        = new SlinqyQueueShard(this.fakePhysicalQueue);
            var validBatch        = new List<string> { "message 1", "message 2" };

            // Act
            await queueShard.SendBatch(validBatch);

            // Assert
            A.CallTo(() =>
                this.fakePhysicalQueue.SendBatch(A<IEnumerable<object>>.Ignored)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the ShardIndex property returns the shards index based on the physical shards name.
        /// </summary>
        [Fact]
        public void ShardIndex_Always_ReturnsIndexFromName()
        {
            // Arrange
            A.CallTo(() =>
                this.fakePhysicalQueue.Name
            ).Returns("queue-100");

            var shard = new SlinqyQueueShard(this.fakePhysicalQueue);

            // Act
            var actualShardIndex = shard.ShardIndex;

            // Assert
            Assert.Equal(100, actualShardIndex);
        }
    }
}
