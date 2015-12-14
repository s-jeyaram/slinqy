namespace Slinqy.Core.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using FakeItEasy;
    using FakeItEasy.Core;
    using Xunit;

    /// <summary>
    /// Tests the public methods exposed by the SlinqyAgent class.
    /// </summary>
    public class SlinqyAgentTests
    {
        /// <summary>
        /// Represents a valid value where one is needed for a Slinqy queue name parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const string ValidSlinqyQueueName = "queue-name";

        /// <summary>
        /// Represents a valid value where one is needed for queue shard index parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const int ValidShardIndex = 0;

        /// <summary>
        /// Represents a valid value where one is needed for max size parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const int ValidMaxSizeMegabytes = 1024;

        /// <summary>
        /// Represents a valid value where one is needed for scale out parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const double ValidStorageCapacityScaleOutThreshold = 0.80;

        /// <summary>
        /// A fake queue service to be used for test purposes.
        /// </summary>
        private readonly IPhysicalQueueService fakeQueueService = A.Fake<IPhysicalQueueService>();

        /// <summary>
        /// The SlinqyAgent instance being tested.
        /// </summary>
        private readonly SlinqyAgent slinqyAgent;

        /// <summary>
        /// A fake SlinqyQueueShard that is configured to be writable.
        /// </summary>
        private readonly SlinqyQueueShard fakeSendableShard;

        /// <summary>
        /// A fake SlinqyQueueShardMonitor.
        /// </summary>
        private readonly SlinqyQueueShardMonitor fakeQueueShardMonitor;

        /// <summary>
        /// The list of fake shards that the fakeQueueShardMonitor works from.
        /// </summary>
        private readonly List<SlinqyQueueShard> fakeShards;

        /// <summary>
        /// Maintains a count of the number of shards that have been created to simplify generating of shard indexes and names.
        /// </summary>
        private int shardIndexCounter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyAgentTests"/> class with default/common behaviors and values.
        /// </summary>
        public
        SlinqyAgentTests()
        {
            // Configures one read/writable shard since that's the minimum valid state.
            this.fakeSendableShard = this.CreateFakeSendReceiveQueue();

            // Configures the fake shard monitor to use the fake shards.
            this.fakeQueueShardMonitor = A.Fake<SlinqyQueueShardMonitor>();
            this.fakeShards = new List<SlinqyQueueShard> { this.fakeSendableShard };

            A.CallTo(() => this.fakeQueueShardMonitor.QueueName).Returns(ValidSlinqyQueueName);
            A.CallTo(() => this.fakeQueueShardMonitor.Shards).Returns(this.fakeShards);

            // Configure the agent that will be tested.
            this.slinqyAgent = new SlinqyAgent(
                queueService:                       this.fakeQueueService,
                slinqyQueueShardMonitor:            this.fakeQueueShardMonitor,
                storageCapacityScaleOutThreshold:   ValidStorageCapacityScaleOutThreshold
            );
        }

        /// <summary>
        /// Verifies that when the current write shard exceeds the defined capacity
        /// threshold that the SlinqyAgent takes action to add a new shard.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_WriteQueueShardExceedsCapacityThreshold_ASendOnlyShardIsAdded()
        {
            // Arrange
            var scaleOutSizeMegabytes = Math.Ceiling(ValidMaxSizeMegabytes * ValidStorageCapacityScaleOutThreshold);
            var scaleOutSizeBytes     = Convert.ToInt64(scaleOutSizeMegabytes * 1024 * 1024);

            A.CallTo(() => this.fakeSendableShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateSendOnlyQueue(A<string>.Ignored)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the agent doesn't add shards when the write
        /// queues size increases but remains under the scale up threshold.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_WriteQueueShardSizeUnderCapacityThreshold_AnotherShardIsNotAdded()
        {
            // Arrange
            var scaleOutSizeMegabytes = Math.Floor(ValidMaxSizeMegabytes * ValidStorageCapacityScaleOutThreshold);
            var sizeBytes             = Convert.ToInt64(scaleOutSizeMegabytes * 1024 * 1024);

            A.CallTo(() => this.fakeSendableShard.PhysicalQueue.CurrentSizeBytes).Returns(sizeBytes);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateQueue(A<string>.Ignored)
            ).MustNotHaveHappened();
        }

        /// <summary>
        /// Verifies that the name of the new shard is correct.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_AnotherShardAdded_ShardNameIsCorrect()
        {
            // Arrange
            var scaleOutSizeMegabytes   = Math.Ceiling(ValidMaxSizeMegabytes * ValidStorageCapacityScaleOutThreshold);
            var scaleOutSizeBytes       = Convert.ToInt64(scaleOutSizeMegabytes * 1024 * 1024);

            A.CallTo(() => this.fakeSendableShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateSendOnlyQueue(ValidSlinqyQueueName + "1")
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the previous shard is set to be receive only after a new write shard is added.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_AnotherShardAdded_PreviousShardIsSetToReceiveOnly()
        {
            // Arrange
            var scaleOutSizeMegabytes = Math.Ceiling(ValidMaxSizeMegabytes * ValidStorageCapacityScaleOutThreshold);
            var scaleOutSizeBytes     = Convert.ToInt64(scaleOutSizeMegabytes * 1024 * 1024);

            // Configure the write shards size to trigger scaling.
            A.CallTo(() => this.fakeSendableShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);

            // Configure the new shard that will be added as a result of the scaling.
            var fakeAdditionalShard = this.CreateFakeSendOnlyQueue();

            A.CallTo(() => this.fakeQueueService.CreateSendOnlyQueue(A<string>.Ignored))
                .Invokes(() => this.fakeShards.Add(fakeAdditionalShard))
                .Returns(fakeAdditionalShard.PhysicalQueue);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueReceiveOnly(this.fakeSendableShard.PhysicalQueue.Name)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that shards existing in between the read and write shard are disabled.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_AnotherShardAddedWithMiddleShards_MiddleShardsAreDisabled()
        {
            // Arrange
            var scaleOutSizeMegabytes = Math.Ceiling(ValidMaxSizeMegabytes * ValidStorageCapacityScaleOutThreshold);
            var scaleOutSizeBytes     = Convert.ToInt64(scaleOutSizeMegabytes * 1024 * 1024);

            var fakeSendableQueue = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => fakeSendableQueue.ShardIndex).Returns(0);
            A.CallTo(() => fakeSendableQueue.PhysicalQueue.Name).Returns(ValidSlinqyQueueName + 0);
            A.CallTo(() => fakeSendableQueue.PhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeSendableQueue.PhysicalQueue.CurrentSizeBytes).Returns(1);

            A.CallTo(() => this.fakeSendableShard.ShardIndex).Returns(1);
            A.CallTo(() => this.fakeSendableShard.PhysicalQueue.IsSendEnabled).Returns(true);

            var shards = new List<SlinqyQueueShard> {
                fakeSendableQueue,
                this.fakeSendableShard
            };

            A.CallTo(() => this.fakeSendableShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);
            A.CallTo(() => this.fakeQueueShardMonitor.Shards).Returns(shards);

            var newWritableShard = A.Fake<IPhysicalQueue>();

            A.CallTo(() => newWritableShard.Name).Returns(ValidSlinqyQueueName + (this.fakeSendableShard.ShardIndex + 1));
            A.CallTo(() => newWritableShard.IsSendEnabled).Returns(true);

            var newShards = new List<IPhysicalQueue> {
                fakeSendableQueue.PhysicalQueue,
                this.fakeSendableShard.PhysicalQueue,
                newWritableShard
            };

            A.CallTo(() => this.fakeQueueService.ListQueues(ValidSlinqyQueueName)).Returns(newShards);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueDisabled(this.fakeSendableShard.PhysicalQueue.Name)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Creates a new instance of SlinqyQueueShard as a fake that is configured to simulate a send/receive queue.
        /// </summary>
        /// <returns>Returns a fake that simulates a send/receive queue.</returns>
        private
        SlinqyQueueShard
        CreateFakeSendReceiveQueue()
        {
            return this.CreateFakeShard(sendable: true, sendReceiveable: true);
        }

        /// <summary>
        /// Creates a new instance of SlinqyQueueShard as a fake that is configured to simulate a send-only queue.
        /// </summary>
        /// <returns>
        /// Returns a fake that simulates a new queue shard.
        /// </returns>
        private
        SlinqyQueueShard
        CreateFakeSendOnlyQueue()
        {
            return this.CreateFakeShard(sendable: true, sendReceiveable: false);
        }

        /// <summary>
        /// Creates a new instance of SlinqyQueueShard as a fake that is configured to simulate a send-only queue.
        /// </summary>
        /// <returns>
        /// Returns a fake that simulates a new queue shard.
        /// </returns>
        private
        SlinqyQueueShard
        CreateFakeReceiveOnlyQueue()
        {
            return this.CreateFakeShard(sendable: false, sendReceiveable: false);
        }

        /// <summary>
        /// Creates a new fake SlinqyQueueShard.
        /// </summary>
        /// <param name="sendable">Specifies if the fake shard can be sent to.</param>
        /// <param name="sendReceiveable">Specifies if the fake shard can be received from.</param>
        /// <param name="maxSizeMegabytes">Specifies the max size of the fake shard.</param>
        /// <returns>Returns the fake shard.</returns>
        private
        SlinqyQueueShard
        CreateFakeShard(
            bool sendable           = true,
            bool sendReceiveable    = true,
            long maxSizeMegabytes   = ValidMaxSizeMegabytes)
        {
            var fakeShard           = A.Fake<SlinqyQueueShard>();
            var fakePhysicalQueue   = fakeShard.PhysicalQueue;

            A.CallTo(() => fakeShard.ShardIndex                 ).Returns(this.shardIndexCounter++);
            A.CallTo(() => fakePhysicalQueue.Name               ).Returns(ValidSlinqyQueueName + fakeShard.ShardIndex);
            A.CallTo(() => fakePhysicalQueue.IsSendEnabled      ).Returns(sendable);
            A.CallTo(() => fakePhysicalQueue.ReadWritable       ).Returns(sendReceiveable);
            A.CallTo(() => fakePhysicalQueue.MaxSizeMegabytes   ).Returns(maxSizeMegabytes);

            return fakeShard;
        }
    }
}