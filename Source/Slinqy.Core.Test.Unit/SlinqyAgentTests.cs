namespace Slinqy.Core.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
        private static readonly string ValidShardPhysicalQueueName = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", ValidSlinqyQueueName, ValidShardIndex);

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
        /// Initializes a new instance of the <see cref="SlinqyAgentTests"/> class with default/common behaviors and values.
        /// </summary>
        public
        SlinqyAgentTests()
        {
            // Configures one writable shard since that's the minimum valid state.
            this.fakeWritableShard = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => this.fakeWritableShard.Writable).Returns(true);
            A.CallTo(() => this.fakeWritableShard.ShardIndex).Returns(ValidShardIndex);
            A.CallTo(() => this.fakeWritableShard.ShardName).Returns(ValidShardPhysicalQueueName);
            A.CallTo(() => this.fakeWritableShard.MaxSizeMegabytes).Returns(ValidMaxSizeMegabytes);

            // Configures the fake shard monitor to use the fake shards.
            var fakeQueueShardMonitor = A.Fake<SlinqyQueueShardMonitor>();
            A.CallTo(() => fakeQueueShardMonitor.Shards).Returns(new List<SlinqyQueueShard> { this.fakeWritableShard });

            // Configure the agent that will be tested.
            this.slinqyAgent = new SlinqyAgent(
                queueService:                       this.fakeQueueService,
                slinqyQueueShardMonitor:            fakeQueueShardMonitor,
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
        SlinqyAgent_WriteQueueShardExceedsCapacityThreshold_AnotherShardIsAdded()
        {
            // Arrange
            var scaleOutSizeMegabytes = Math.Ceiling(ValidMaxSizeMegabytes * ValidStorageCapacityScaleOutThreshold);
            var scaleOutSizeBytes     = Convert.ToInt64(scaleOutSizeMegabytes * 1024 * 1024);

            A.CallTo(() => this.fakeWritableShard.CurrentSizeBytes).Returns(scaleOutSizeBytes);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateQueue(A<string>.Ignored)
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

            A.CallTo(() => this.fakeWritableShard.CurrentSizeBytes).Returns(sizeBytes);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateQueue(A<string>.Ignored)
            ).MustNotHaveHappened();
        }
    }
}