namespace Slinqy.Core.Test.Unit
{
    using System;
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
        /// Represents a valid value where one is needed for max wait timespan parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private readonly TimeSpan validMaxWaitTimeSpan = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The fake that simulates a queue shard monitor.
        /// </summary>
        private readonly SlinqyQueueShardMonitor fakeQueueShardMonitor = A.Fake<SlinqyQueueShardMonitor>();

        /// <summary>
        /// The fake that simulates a receive-only queue shard.
        /// </summary>
        private readonly SlinqyQueueShard fakeReceiveShard;

        /// <summary>
        /// The fake that simulates a writable queue shard.
        /// </summary>
        private readonly SlinqyQueueShard fakeSendShard;

        /// <summary>
        /// The instance under test.
        /// </summary>
        private readonly SlinqyQueue slinqyQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueTests"/> class.
        /// </summary>
        public
        SlinqyQueueTests()
        {
            this.fakeReceiveShard   = A.Fake<SlinqyQueueShard>();
            this.fakeSendShard      = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => this.fakeReceiveShard.PhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => this.fakeSendShard.PhysicalQueue.IsSendEnabled).Returns(true);

            var shards = new List<SlinqyQueueShard> {
                this.fakeReceiveShard,
                this.fakeSendShard
            };

            A.CallTo(() => this.fakeQueueShardMonitor.Shards).Returns(shards);
            A.CallTo(() => this.fakeQueueShardMonitor.SendShard).Returns(this.fakeSendShard);

            this.slinqyQueue = new SlinqyQueue(
                this.fakeQueueShardMonitor
            );
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
            A.CallTo(() => this.fakeReceiveShard.PhysicalQueue.MaxSizeMegabytes).Returns(1024);
            A.CallTo(() => this.fakeSendShard.PhysicalQueue.MaxSizeMegabytes).Returns(1024);

            // Act
            var actual = this.slinqyQueue.MaxQueueSizeMegabytes;

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
            A.CallTo(() => this.fakeReceiveShard.PhysicalQueue.CurrentSizeBytes).Returns(1);
            A.CallTo(() => this.fakeSendShard.PhysicalQueue.CurrentSizeBytes).Returns(2);

            // Act
            var actual = this.slinqyQueue.CurrentQueueSizeBytes;

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
            var validBatch = new List<string> { "message 1", "message 2" };

            // Act
            await this.slinqyQueue.SendBatch(validBatch);

            // Assert
            A.CallTo(() =>
                this.fakeSendShard.SendBatch(A<IEnumerable<object>>.Ignored)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the SlinqyQueue uses the ReceiveShard to perform receive operations.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        ReceiveBatch_Always_CallsReceiveBatchOnReceiveShard()
        {
            // Arrange
            A.CallTo(() =>
                this.fakeQueueShardMonitor.ReceiveShard
            ).Returns(this.fakeReceiveShard);

            // Act
            await this.slinqyQueue.ReceiveBatch(this.validMaxWaitTimeSpan);

            // Assert
            A.CallTo(() =>
                this.fakeReceiveShard.ReceiveBatch(A<TimeSpan>.Ignored)
            ).MustHaveHappened();
        }
    }
}