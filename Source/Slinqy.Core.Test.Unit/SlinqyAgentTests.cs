namespace Slinqy.Core.Test.Unit
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// Tests the public methods exposed by the SlinqyAgent class.
    /// </summary>
    public class SlinqyAgentTests
    {
        /// <summary>
        /// Represents a valid value where a one is needed for max size parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const int ValidMaxSizeMegabytes = 1024;

        /// <summary>
        /// Maintains a count of the number of times the FakeListPhysicalQueueAutoGrow method is called.
        /// </summary>
        private int fakeListPhysicalQueueAutoGrowCallCount = 1;

        /// <summary>
        /// Verifies that when the current write shard exceeds the defined capacity
        /// threshold that the SlinqyAgent takes action to add a new shard.
        /// </summary>
        [Fact]
        public
        void
        SlinqyAgent_QueueShardExceedsCapacityThreshold_AnotherShardIsAdded()
        {
            // Arrange
            var createCalled = false;

            var slinqyAgent = new SlinqyAgent(
                createPhysicalQueueDelegate:        queueShardName => Task.Run(() => {
                                                        createCalled = true;

                                                        return new SlinqyQueueShard(
                                                            queueShardName,
                                                            ValidMaxSizeMegabytes,
                                                            0
                                                        );
                                                    }),
                listPhysicalQueuesDelegate:         this.FakeListPhysicalQueueAutoGrow,
                storageCapacityScaleOutThreshold:   0.25
            );

            // Act
            slinqyAgent.Start();

            // Assert
            Assert.True(createCalled);
        }

        /// <summary>
        /// A ListPhysicalQueuesDelegate implementation that will return one physical queue shard
        /// while increasing the size of the shard after each call to simulate a growing queue.
        /// The queue will grow in steps of 25% of the max size, so it will reach full after four calls.
        /// </summary>
        /// <param name="queueNamePrefix">
        /// Specifies a queue name prefix to search for.
        /// </param>
        /// <returns>Returns </returns>
        private
        Task<IEnumerable<SlinqyQueueShard>>
        FakeListPhysicalQueueAutoGrow(
            string queueNamePrefix)
        {
            var currentCount = this.fakeListPhysicalQueueAutoGrowCallCount;

            if (currentCount < 4)
                this.fakeListPhysicalQueueAutoGrowCallCount++;

            var currentSizeMegabytes = ValidMaxSizeMegabytes / currentCount;

            return Task.Run(() => new List<SlinqyQueueShard> {
                new SlinqyQueueShard(queueNamePrefix, ValidMaxSizeMegabytes, currentSizeMegabytes)
            }.AsEnumerable());
        }
    }
}