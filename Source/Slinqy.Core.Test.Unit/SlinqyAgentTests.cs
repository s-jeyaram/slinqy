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
        /// Represents a valid value where one is needed for a physical queue name parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private static readonly string ValidShardPhysicalQueueName = string.Format(CultureInfo.InvariantCulture, "{0}{1}", ValidSlinqyQueueName, ValidShardIndex);

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
        private readonly SlinqyQueueShard fakeWritableShard;

        /// <summary>
        /// A fake SlinqyQueueShardMonitor.
        /// </summary>
        private readonly SlinqyQueueShardMonitor fakeQueueShardMonitor;

        /// <summary>
        /// The list of fake shards that the fakeQueueShardMonitor works from.
        /// </summary>
        private List<SlinqyQueueShard> fakeShards;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyAgentTests"/> class with default/common behaviors and values.
        /// </summary>
        public
        SlinqyAgentTests()
        {
            // Configures one writable shard since that's the minimum valid state.
            this.fakeWritableShard = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => this.fakeWritableShard.PhysicalQueue.Writable).Returns(true);
            A.CallTo(() => this.fakeWritableShard.ShardIndex).Returns(ValidShardIndex);
            A.CallTo(() => this.fakeWritableShard.PhysicalQueue.Name).Returns(ValidShardPhysicalQueueName);
            A.CallTo(() => this.fakeWritableShard.PhysicalQueue.MaxSizeMegabytes).Returns(ValidMaxSizeMegabytes);

            // Configures the fake shard monitor to use the fake shards.
            this.fakeQueueShardMonitor = A.Fake<SlinqyQueueShardMonitor>();
            this.fakeShards = new List<SlinqyQueueShard> { this.fakeWritableShard };

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

            A.CallTo(() => this.fakeWritableShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);

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

            A.CallTo(() => this.fakeWritableShard.PhysicalQueue.CurrentSizeBytes).Returns(sizeBytes);

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

            A.CallTo(() => this.fakeWritableShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);

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

            A.CallTo(() => this.fakeWritableShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);

            A.CallTo(() => this.fakeQueueService.CreateSendOnlyQueue(A<string>.Ignored))
                .Invokes(this.AddFakeSendOnlyQueue)
                .Returns(this.fakeShards.Last().PhysicalQueue);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueReceiveOnly(this.fakeWritableShard.PhysicalQueue.Name)
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

            var fakeReadableShard = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => fakeReadableShard.ShardIndex).Returns(0);
            A.CallTo(() => fakeReadableShard.PhysicalQueue.Name).Returns(ValidSlinqyQueueName + 0);
            A.CallTo(() => fakeReadableShard.PhysicalQueue.Writable).Returns(false);
            A.CallTo(() => fakeReadableShard.PhysicalQueue.CurrentSizeBytes).Returns(1);

            A.CallTo(() => this.fakeWritableShard.ShardIndex).Returns(1);
            A.CallTo(() => this.fakeWritableShard.PhysicalQueue.Writable).Returns(true);

            var shards = new List<SlinqyQueueShard> {
                fakeReadableShard,
                this.fakeWritableShard
            };

            A.CallTo(() => this.fakeWritableShard.PhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);
            A.CallTo(() => this.fakeQueueShardMonitor.Shards).Returns(shards);

            var newWritableShard = A.Fake<IPhysicalQueue>();

            A.CallTo(() => newWritableShard.Name).Returns(ValidSlinqyQueueName + (this.fakeWritableShard.ShardIndex + 1));
            A.CallTo(() => newWritableShard.Writable).Returns(true);

            var newShards = new List<IPhysicalQueue> {
                fakeReadableShard.PhysicalQueue,
                this.fakeWritableShard.PhysicalQueue,
                newWritableShard
            };

            A.CallTo(() => this.fakeQueueService.ListQueues(ValidSlinqyQueueName)).Returns(newShards);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueDisabled(this.fakeWritableShard.PhysicalQueue.Name)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Adds a new send-only fake queue to the monitors list of shards.
        /// </summary>
        /// <param name="fakeObjectCall">
        /// Specifies parameters of the call to create the queue.
        /// </param>
        private
        void
        AddFakeSendOnlyQueue(
            IFakeObjectCall fakeObjectCall)
        {
            var fakeSendOnlyQueue = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => fakeSendOnlyQueue.PhysicalQueue.Name).Returns(fakeObjectCall.GetArgument<string>(argumentName: "name"));
            A.CallTo(() => fakeSendOnlyQueue.PhysicalQueue.Writable).Returns(true);
            A.CallTo(() => fakeSendOnlyQueue.PhysicalQueue.ReadWritable).Returns(false);
            A.CallTo(() => fakeSendOnlyQueue.PhysicalQueue.MaxSizeMegabytes).Returns(ValidMaxSizeMegabytes);

            this.fakeShards.Add(fakeSendOnlyQueue);
        }
    }
}