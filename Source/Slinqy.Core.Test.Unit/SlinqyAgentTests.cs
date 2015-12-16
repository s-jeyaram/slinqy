namespace Slinqy.Core.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
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
        private readonly SlinqyQueueShard fakeShard;

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
            this.fakeShard = this.CreateFakeSendReceiveQueue();

            // Configures the fake shard monitor to use the fake shards.
            this.fakeQueueShardMonitor = A.Fake<SlinqyQueueShardMonitor>();
            this.fakeShards = new List<SlinqyQueueShard> { this.fakeShard };

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

            A.CallTo(() => this.fakeShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);

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

            A.CallTo(() => this.fakeShard.PhysicalQueue.CurrentSizeBytes).Returns(sizeBytes);

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

            A.CallTo(() => this.fakeShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);

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
            A.CallTo(() => this.fakeShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);

            // Configure the new shard that will be added as a result of the scaling.
            var fakeAdditionalShard = this.CreateFakeSendOnlyQueue();

            A.CallTo(() => this.fakeQueueService.CreateSendOnlyQueue(A<string>.Ignored))
                .Invokes(() => this.fakeShards.Add(fakeAdditionalShard))
                .Returns(fakeAdditionalShard.PhysicalQueue);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueReceiveOnly(this.fakeShard.PhysicalQueue.Name)
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

            // Configure the initial default shard to be receive-only.
            A.CallTo(() => this.fakeShard.PhysicalQueue.IsSendEnabled).Returns(false);

            // Create a new send-only shard that has exceeded the scale out threshold,
            // this will become the middle shard after scaling occurs.
            var sendOnlyShard = this.CreateFakeSendOnlyQueue(currentSizeBytes: scaleOutSizeBytes);
            this.fakeShards.Add(sendOnlyShard);

            // Configure the new shard that will be added as a result of the scaling.
            var newSendOnlyShard = this.CreateFakeSendOnlyQueue();

            A.CallTo(() => this.fakeQueueService.CreateSendOnlyQueue(A<string>.Ignored))
                .Invokes(() => this.fakeShards.Add(newSendOnlyShard))
                .Returns(newSendOnlyShard.PhysicalQueue);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueDisabled(this.fakeShard.PhysicalQueue.Name)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that once a shard has been disabled the agent doesn't continually try to disable it.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_ShardsAlreadyDisabled_DoesNotDisableAgain()
        {
            // Arrange
            // Configure the initial default shard to be receive-only.
            A.CallTo(() => this.fakeShard.PhysicalQueue.CurrentSizeBytes).Returns(1);
            A.CallTo(() => this.fakeShard.PhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => this.fakeShard.PhysicalQueue.IsReceiveEnabled).Returns(true);

            var fakeDisabledMiddleShard = this.CreateFakeDisabledQueue();
            var fakeSendOnlyShard       = this.CreateFakeSendOnlyQueue();

            this.fakeShards.Add(fakeDisabledMiddleShard);
            this.fakeShards.Add(fakeSendOnlyShard);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueDisabled(A<string>.Ignored)
            ).MustNotHaveHappened();
        }

        /// <summary>
        /// Verifies that the agent will retry a scale out operation after encountering unhandled exceptions.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_ExceptionOccursDuringScaleOut_Retries()
        {
            // Arrange
            var scaleOutSizeMegabytes = Math.Ceiling(ValidMaxSizeMegabytes * ValidStorageCapacityScaleOutThreshold);
            var scaleOutSizeBytes     = Convert.ToInt64(scaleOutSizeMegabytes * 1024 * 1024);

            // Configure the write shards size to trigger scaling.
            A.CallTo(() => this.fakeShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);
            A.CallTo(() => this.fakeQueueService.CreateSendOnlyQueue(A<string>.Ignored)).Throws<Exception>().Once();

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateSendOnlyQueue(A<string>.Ignored)
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
            return this.CreateFakeShard(sendable: true, receivable: true);
        }

        /// <summary>
        /// Creates a new instance of SlinqyQueueShard as a fake that is configured to simulate a send-only queue.
        /// </summary>
        /// <param name="currentSizeBytes">Specifies the current size of the fake shard.</param>
        /// <returns>
        /// Returns a fake that simulates a new queue shard.
        /// </returns>
        private
        SlinqyQueueShard
        CreateFakeSendOnlyQueue(
            long currentSizeBytes = 0)
        {
            return this.CreateFakeShard(
                sendable:           true,
                receivable:         false,
                currentSizeBytes:   currentSizeBytes
            );
        }

        /// <summary>
        /// Creates a new instance of SlinqyQueueShard as a fake that is configured to simulate a disabled queue.
        /// </summary>
        /// <returns>
        /// Returns a fake that simulates a new queue shard.
        /// </returns>
        private
        SlinqyQueueShard
        CreateFakeDisabledQueue()
        {
            return this.CreateFakeShard(sendable: false, receivable: false);
        }

        /// <summary>
        /// Creates a new fake SlinqyQueueShard.
        /// </summary>
        /// <param name="sendable">Specifies if the fake shard can be sent to.</param>
        /// <param name="receivable">Specifies if the fake shard can be received from.</param>
        /// <param name="maxSizeMegabytes">Specifies the max size of the fake shard.</param>
        /// <param name="currentSizeBytes">Specifies the current size of the fake shard.</param>
        /// <returns>Returns the fake shard.</returns>
        private
        SlinqyQueueShard
        CreateFakeShard(
            bool sendable           = true,
            bool receivable         = true,
            long maxSizeMegabytes   = ValidMaxSizeMegabytes,
            long currentSizeBytes   = 0)
        {
            var fakeShard           = A.Fake<SlinqyQueueShard>();
            var fakePhysicalQueue   = fakeShard.PhysicalQueue;

            A.CallTo(() => fakeShard.ShardIndex                 ).Returns(this.shardIndexCounter++);
            A.CallTo(() => fakeShard.IsDisabled                 ).Returns(!sendable && !receivable);
            A.CallTo(() => fakePhysicalQueue.Name               ).Returns(ValidSlinqyQueueName + fakeShard.ShardIndex);
            A.CallTo(() => fakePhysicalQueue.IsSendEnabled      ).Returns(sendable);
            A.CallTo(() => fakePhysicalQueue.IsReceiveEnabled   ).Returns(receivable);
            A.CallTo(() => fakePhysicalQueue.MaxSizeMegabytes   ).Returns(maxSizeMegabytes);
            A.CallTo(() => fakePhysicalQueue.CurrentSizeBytes   ).Returns(currentSizeBytes);

            return fakeShard;
        }
    }
}