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
        /// Represents a valid value where one is needed for a Slinqy queue name parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const string ValidSlinqyQueueName = "queue-name";

        /// <summary>
        /// Represents a valid value where one is needed for a Slinqy shard name parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const string ValidSlinqyShardName = ValidSlinqyQueueName + "0";

        /// <summary>
        /// Represents a valid value where one is needed for max wait timespan parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private readonly TimeSpan validMaxWaitTimeSpan = TimeSpan.FromSeconds(1);

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
        /// Verifies that the GenerateFirstShardName returns the correct
        /// shard name when index padding is specified.
        /// </summary>
        [Fact]
        public
        static
        void
        GenerateFirstShardName_MultipleShardIndexPaddingIsSpecified_ReturnsCorrectName()
        {
            // Arrange
            const int ShardIndexPadding = 2;

            // Act
            var actual = SlinqyQueueShard.GenerateFirstShardName(ValidSlinqyQueueName, ShardIndexPadding);

            // Assert
            Assert.Equal(ValidSlinqyQueueName + "00", actual);
        }

        /// <summary>
        /// Verifies that GenerateNextShardName properly handles zero padding in shard names from existing shard names.
        /// </summary>
        [Fact]
        public
        static
        void
        GenerateNextShardName_ZeroPaddedShardName_ReturnsIncrementedShardNameWithMatchingPadding()
        {
            // Arrange
            const string ExistingShardName = ValidSlinqyQueueName + "0001";

            // Act
            var actual = SlinqyQueueShard.GenerateNextShardName(ExistingShardName);

            // Assert
            Assert.Equal(ValidSlinqyQueueName + "0002", actual);
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
        /// Verifies that the Send method properly submits the message to the underlying physical queue.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public
        async Task
        Send_MessageIsValid_MessageSentToPhysicalQueue()
        {
            // Arrange
            var validMessage = "message 1";

            // Act
            await this.slinqyQueueShard.Send(validMessage);

            // Assert
            A.CallTo(() =>
                this.fakePhysicalQueue.Send(validMessage)
            ).MustHaveHappened();
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
            ).Returns("queue100");

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

        /// <summary>
        /// Verifies that the IsDisabled property returns true when both sending
        /// and receiving is disabled on the underlying physical queue.
        /// </summary>
        [Fact]
        public
        void
        IsDisabled_SendDisabledReceiveDisabled_ReturnsTrue()
        {
            // Arrange
            A.CallTo(() => this.fakePhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => this.fakePhysicalQueue.IsReceiveEnabled).Returns(false);

            // Act
            var actualValue = this.slinqyQueueShard.IsDisabled;

            // Assert
            Assert.True(actualValue);
        }

        /// <summary>
        /// Verifies that Receive calls the underlying IPhysicalQueue method.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        Receive_Always_CallsReceiveOnPhysicalQueue()
        {
            // Arrange
            // Act
            await this.slinqyQueueShard.Receive<string>();

            // Assert
            A.CallTo(() =>
                this.fakePhysicalQueue.Receive<string>()
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that ReceiveBatch correctly calls the underlying IPhysicalQueue method.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        ReceiveBatch_Always_CallsReceiveBatchOnPhysicalQueue()
        {
            // Arrange
            // Act
            await this.slinqyQueueShard.ReceiveBatch(this.validMaxWaitTimeSpan);

            // Assert
            A.CallTo(() =>
                this.fakePhysicalQueue.ReceiveBatch(A<TimeSpan>.Ignored)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the IsReceiveOnly property returns False when
        /// both send and receive are disabled on the underlying physical queue.
        /// </summary>
        [Fact]
        public
        void
        IsReceiveOnly_SendAndReceiveDisabledOnPhysicalQueue_ReturnsFalse()
        {
            // Arrange
            A.CallTo(() => this.fakePhysicalQueue.IsReceiveEnabled).Returns(false);
            A.CallTo(() => this.fakePhysicalQueue.IsSendEnabled).Returns(false);

            // Act
            var actualValue = this.slinqyQueueShard.IsReceiveOnly;

            // Assert
            Assert.False(actualValue);
        }

        /// <summary>
        /// Verifies that the IsReceiveOnly property returns True when
        /// send is disabled and receive is enabled on the underlying physical queue.
        /// </summary>
        [Fact]
        public
        void
        IsReceiveOnly_SendDisabledReceiveEnabledOnPhysicalQueue_ReturnsTrue()
        {
            // Arrange
            A.CallTo(() => this.fakePhysicalQueue.IsReceiveEnabled).Returns(true);
            A.CallTo(() => this.fakePhysicalQueue.IsSendEnabled).Returns(false);

            // Act
            var actualValue = this.slinqyQueueShard.IsReceiveOnly;

            // Assert
            Assert.True(actualValue);
        }

        /// <summary>
        /// Verifies that the IsReceiveOnly property returns False when
        /// send is enabled and receive is enabled on the underlying physical queue.
        /// </summary>
        [Fact]
        public
        void
        IsReceiveOnly_SendEnabledReceiveEnabledOnPhysicalQueue_ReturnsFalse()
        {
            // Arrange
            A.CallTo(() => this.fakePhysicalQueue.IsReceiveEnabled).Returns(true);
            A.CallTo(() => this.fakePhysicalQueue.IsSendEnabled).Returns(true);

            // Act
            var actualValue = this.slinqyQueueShard.IsReceiveOnly;

            // Assert
            Assert.False(actualValue);
        }

        /// <summary>
        /// Verifies that the IsReceiveOnly property returns False when
        /// send is enabled and receive is disabled on the underlying physical queue.
        /// </summary>
        [Fact]
        public
        void
        IsReceiveOnly_SendEnabledReceiveDisabledOnPhysicalQueue_ReturnsFalse()
        {
            // Arrange
            A.CallTo(() => this.fakePhysicalQueue.IsReceiveEnabled).Returns(false);
            A.CallTo(() => this.fakePhysicalQueue.IsSendEnabled).Returns(true);

            // Act
            var actualValue = this.slinqyQueueShard.IsReceiveOnly;

            // Assert
            Assert.False(actualValue);
        }

        /// <summary>
        /// Verifies that the IsSendReceiveEnabled property returns False when
        /// send is enabled and receive is disabled on the underlying physical queue.
        /// </summary>
        [Fact]
        public
        void
        IsSendReceiveEnabled_SendEnabledReceiveDisabledOnPhysicalQueue_ReturnsFalse()
        {
            // Arrange
            A.CallTo(() => this.fakePhysicalQueue.IsReceiveEnabled).Returns(false);
            A.CallTo(() => this.fakePhysicalQueue.IsSendEnabled).Returns(true);

            // Act
            var actualValue = this.slinqyQueueShard.IsSendReceiveEnabled;

            // Assert
            Assert.False(actualValue);
        }

        /// <summary>
        /// Verifies that the IsSendReceiveEnabled property returns True when
        /// send is enabled and receive is enabled on the underlying physical queue.
        /// </summary>
        [Fact]
        public
        void
        IsSendReceiveEnabled_SendEnabledReceiveEnabledOnPhysicalQueue_ReturnsTrue()
        {
            // Arrange
            A.CallTo(() => this.fakePhysicalQueue.IsReceiveEnabled).Returns(true);
            A.CallTo(() => this.fakePhysicalQueue.IsSendEnabled).Returns(true);

            // Act
            var actualValue = this.slinqyQueueShard.IsSendReceiveEnabled;

            // Assert
            Assert.True(actualValue);
        }

        /// <summary>
        /// Verifies that the IsSendReceiveEnabled property returns False when
        /// send is disabled and receive is enabled on the underlying physical queue.
        /// </summary>
        [Fact]
        public
        void
        IsSendReceiveEnabled_SendDisabledReceiveEnabledOnPhysicalQueue_ReturnsFalse()
        {
            // Arrange
            A.CallTo(() => this.fakePhysicalQueue.IsReceiveEnabled).Returns(true);
            A.CallTo(() => this.fakePhysicalQueue.IsSendEnabled).Returns(false);

            // Act
            var actualValue = this.slinqyQueueShard.IsSendReceiveEnabled;

            // Assert
            Assert.False(actualValue);
        }

        /// <summary>
        /// Verifies that the IsSendReceiveEnabled property returns false when both sending
        /// and receiving is disabled on the underlying physical queue.
        /// </summary>
        [Fact]
        public
        void
        IsSendReceiveEnabled_SendDisabledReceiveDisabled_ReturnsFalse()
        {
            // Arrange
            A.CallTo(() => this.fakePhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => this.fakePhysicalQueue.IsReceiveEnabled).Returns(false);

            // Act
            var actualValue = this.slinqyQueueShard.IsSendReceiveEnabled;

            // Assert
            Assert.False(actualValue);
        }
    }
}
