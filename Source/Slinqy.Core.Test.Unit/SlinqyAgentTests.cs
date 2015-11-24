namespace Slinqy.Core.Test.Unit
{
    using System.Collections.Generic;
    using System.Linq;
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
            var fakeQueues = new List<SlinqyQueueShard> {
                new SlinqyQueueShard(ValidSlinqyQueueName + "1", ValidMaxSizeMegabytes, 0, ValidMaxSizeMegabytes, true)
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(A<string>.Ignored)
            ).Returns(fakeQueues);

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
        SlinqyAgent_WriteQueueShardSizeIncreasesRemainingUnderCapacityThreshold_AnotherShardIsNotAdded()
        {
            // Arrange
            var fakeQueues = new List<SlinqyQueueShard> {
                new SlinqyQueueShard(ValidSlinqyQueueName, 0, ValidMaxSizeMegabytes, 0, true)
            };

            A.CallTo(
                () => this.fakeQueueService.ListQueues(A<string>.Ignored)
            ).Returns(Task.Run(() => fakeQueues.AsEnumerable()));

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateQueue(A<string>.Ignored)
            ).MustNotHaveHappened();
        }
    }
}