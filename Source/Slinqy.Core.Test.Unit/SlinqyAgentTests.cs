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
        /// Maintains a count of the number of times the FakeListPhysicalQueuesAutoGrowWrite method is called.
        /// </summary>
        private int fakeListPhysicalQueuesAutoGrowWriteCallCount = 1;

        /// <summary>
        /// Verifies that when the current write shard exceeds the defined capacity
        /// threshold that the SlinqyAgent takes action to add a new shard.
        /// </summary>
        [Fact]
        public
        void
        SlinqyAgent_WriteQueueShardExceedsCapacityThreshold_AnotherShardIsAdded()
        {
            // Arrange
            var createCalled = false;

            var slinqyAgent = new SlinqyAgent(
                createPhysicalQueueDelegate:        queueShardName => Task.Run(() => {
                                                        createCalled = true;

                                                        return new SlinqyQueueShard(
                                                            queueShardName,
                                                            ValidMaxSizeMegabytes,
                                                            0,
                                                            true
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
        /// Verifies that the agent doesn't add shards when the write
        /// queues size increases but remains under the scale up threshold.
        /// </summary>
        [Fact]
        public
        void
        SlinqyAgent_WriteQueueShardSizeIncreasesRemainingUnderCapacityThreshold_AnotherShardIsNotAdded()
        {
            // Arrange
            var createCalled = false;
            var shards = new List<SlinqyQueueShard> {
                new SlinqyQueueShard("shard-0", ValidMaxSizeMegabytes, 0, true)
            };

            var slinqyAgent = new SlinqyAgent(
                createPhysicalQueueDelegate:        queueShardName => Task.Run(() => {
                                                        createCalled = true;

                                                        return new SlinqyQueueShard(
                                                            queueShardName,
                                                            ValidMaxSizeMegabytes,
                                                            0,
                                                            true
                                                        );
                                                    }),
                listPhysicalQueuesDelegate:         queueNamePrefix => this.FakeListPhysicalQueuesAutoGrowWrite(shards, 0.24),
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
        /// <returns>Returns a fake list of shards.</returns>
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
                new SlinqyQueueShard(queueNamePrefix, ValidMaxSizeMegabytes, currentSizeMegabytes, true)
            }.AsEnumerable());
        }

        /// <summary>
        /// A ListPhysicalQueuesDelegate implementation that provides a list of fake physical queues
        /// and subsequently manipualtes them to simulate various scenarios for testing purposes.
        /// </summary>
        /// <param name="shards">Specifies the shards to auto grow.</param>
        /// <param name="growToPercentOfMax">
        /// Specifies what size to grow the fake write queue shard to after the first listing.
        /// </param>
        /// <returns>Returns a fake list of shards.</returns>
        private
        Task<IEnumerable<SlinqyQueueShard>>
        FakeListPhysicalQueuesAutoGrowWrite(
            IEnumerable<SlinqyQueueShard>   shards,
            double                          growToPercentOfMax)
        {
            var currentCount = this.fakeListPhysicalQueuesAutoGrowWriteCallCount;

            if (currentCount >= 1)
                return Task.Run(() => shards);

            var shardsClone = shards.ToList();

            this.fakeListPhysicalQueuesAutoGrowWriteCallCount++;

            // Find the write queue
            var writeQueue = shardsClone.Single(q => q.Writable);

            // Calculate the new size for the write queue
            var newSizeMegabytes = writeQueue.MaxSizeMegabytes * growToPercentOfMax;
            var newWriteQueue    = new SlinqyQueueShard(
                writeQueue.ShardName,
                writeQueue.MaxSizeMegabytes,
                newSizeMegabytes,
                true
            );

            // Swap out the old instance with an updated one.
            shardsClone.Remove(writeQueue);
            shardsClone.Add(newWriteQueue);

            return Task.Run(() => shardsClone.AsEnumerable());
        }
    }
}