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
        /// Represents a valid value where one is needed for a physical queue name parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const string ValidShardPhysicalQueueName = "queue-name-1";

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
        /// A fake queue configured for writing.
        /// </summary>
        private readonly IPhysicalQueue fakeWritePhysicalQueue = A.Fake<IPhysicalQueue>();

        /// <summary>
        /// The SlinqyAgent instance being tested.
        /// </summary>
        private readonly SlinqyAgent slinqyAgent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyAgentTests"/> class.
        /// </summary>
        public
        SlinqyAgentTests()
        {
            this.slinqyAgent = new SlinqyAgent(
                queueName:                          ValidSlinqyQueueName,
                queueService:                       this.fakeQueueService,
                storageCapacityScaleOutThreshold:   ValidStorageCapacityScaleOutThreshold
            );

            // Configure default/typical behaviors and values.
            A.CallTo(() => this.fakeWritePhysicalQueue.Writable).Returns(true);
            A.CallTo(() => this.fakeWritePhysicalQueue.Name).Returns(ValidShardPhysicalQueueName);
            A.CallTo(() => this.fakeWritePhysicalQueue.MaxSizeMegabytes).Returns(ValidMaxSizeMegabytes);
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
            var fakeQueues = new List<IPhysicalQueue> {
                this.fakeWritePhysicalQueue
            };

            // Calculate minimum size to trigger scaling.
            var scaleOutSizeMegabytes = Math.Ceiling(ValidMaxSizeMegabytes * ValidStorageCapacityScaleOutThreshold);
            var scaleOutSizeBytes     = Convert.ToInt64(scaleOutSizeMegabytes * 1024 * 1024);

            A.CallTo(() => this.fakeWritePhysicalQueue.CurrentSizeBytes).Returns(scaleOutSizeBytes);
            A.CallTo(() => this.fakeQueueService.ListQueues(ValidSlinqyQueueName)).Returns(fakeQueues);

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
            var fakeQueues = new List<IPhysicalQueue> {
                this.fakeWritePhysicalQueue
            };

            A.CallTo(() => this.fakeQueueService.ListQueues(ValidSlinqyQueueName)).Returns(fakeQueues);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateQueue(A<string>.Ignored)
            ).MustNotHaveHappened();
        }
    }
}