namespace Slinqy.Core.Test.Unit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    /// <summary>
    /// Tests functionality exposed by the SlinqyQueueShardMonitor class.
    /// </summary>
    public class SlinqyQueueShardMonitorTests
    {
        private const string ValidSlinqyQueueName = "queue-name";

        private readonly IPhysicalQueueService fakeQueueService = A.Fake<IPhysicalQueueService>();

        private readonly SlinqyQueueShardMonitor monitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueShardMonitorTests"/> class.
        /// </summary>
        public
        SlinqyQueueShardMonitorTests()
        {
            this.monitor = new SlinqyQueueShardMonitor(
                ValidSlinqyQueueName,
                this.fakeQueueService
            );
        }

        /// <summary>
        /// Verifies that physical queue shards are correctly represented as SlinqyQueueShards.
        /// </summary>
        /// <returns>Returns the async Task for the test.</returns>
        [Fact]
        public
        async Task
        SlinqyQueueShardMonitor_PhysicalQueueShardsExists_MatchingSlinqyQueueShardsExist()
        {
            // Arrange
            var fakePhysicalQueue1 = A.Fake<SlinqyQueueShard>();
            var fakePhysicalQueue2 = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => fakePhysicalQueue1.ShardName).Returns(ValidSlinqyQueueName + "1");
            A.CallTo(() => fakePhysicalQueue2.ShardName).Returns(ValidSlinqyQueueName + "2");

            var physicalQueues = new List<SlinqyQueueShard> {
                fakePhysicalQueue1,
                fakePhysicalQueue2
            };

            A.CallTo(() => this.fakeQueueService.ListQueues(ValidSlinqyQueueName))
                .Returns(physicalQueues);

            // Act
            await this.monitor.Start();                  // Start monitoring for shards.

            var slinqyQueueShards = this.monitor.Shards; // Retrieve shards that were detected.

            // Assert
            Assert.Equal(physicalQueues, slinqyQueueShards);
        }

        /// <summary>
        /// Verifies that the WriteShard property reflects the current write shard.
        /// </summary>
        /// <returns>Returns the async Task for the test.</returns>
        [Fact]
        public
        async Task
        WriteShard_OneReadWriteShardExists_SlinqyQueueWriteShardIsReturned()
        {
            // Arrange
            var fakeReadWritePhysicalQueue = A.Fake<SlinqyQueueShard>();

            A.CallTo(() =>
                fakeReadWritePhysicalQueue.Writable
            ).Returns(true);

            var physicalQueues = new List<SlinqyQueueShard> {
                fakeReadWritePhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();

            // Assert
            Assert.Equal(
                fakeReadWritePhysicalQueue,
                this.monitor.WriteShard
            );
        }

        /// <summary>
        /// Verifies that the WriteShard property reflects the current write shard.
        /// </summary>
        /// <returns>Returns the async Task for the test.</returns>
        [Fact]
        public
        async Task
        WriteShard_OneReadOneWriteShardExists_SlinqyQueueWriteShardIsReturned()
        {
            // Arrange
            var fakeReadPhysicalQueue  = A.Fake<SlinqyQueueShard>();
            var fakeWritePhysicalQueue = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => fakeReadPhysicalQueue.Writable).Returns(false);
            A.CallTo(() => fakeWritePhysicalQueue.Writable).Returns(true);

            var physicalQueues = new List<SlinqyQueueShard> {
                fakeReadPhysicalQueue,
                fakeWritePhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();

            // Assert
            Assert.Equal(
                fakeWritePhysicalQueue,
                this.monitor.WriteShard
            );
        }

        /// <summary>
        /// Verifies that the WriteShard property reflects the current write shard.
        /// </summary>
        /// <returns>Returns the async Task for the test.</returns>
        [Fact]
        public
        async Task
        WriteShard_OneReadManyDisabledOneWriteShardExists_SlinqyQueueWriteShardIsReturned()
        {
            // Arrange
            var fakeReadPhysicalQueue      = A.Fake<SlinqyQueueShard>();
            var fakeDisabled1PhysicalQueue = A.Fake<SlinqyQueueShard>();
            var fakeDisabled2PhysicalQueue = A.Fake<SlinqyQueueShard>();
            var fakeDisabled3PhysicalQueue = A.Fake<SlinqyQueueShard>();
            var fakeWritePhysicalQueue     = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => fakeReadPhysicalQueue.Writable).Returns(false);
            A.CallTo(() => fakeDisabled1PhysicalQueue.Writable).Returns(false);
            A.CallTo(() => fakeDisabled2PhysicalQueue.Writable).Returns(false);
            A.CallTo(() => fakeDisabled3PhysicalQueue.Writable).Returns(false);
            A.CallTo(() => fakeWritePhysicalQueue.Writable).Returns(true);

            var physicalQueues = new List<SlinqyQueueShard> {
                fakeReadPhysicalQueue,
                fakeWritePhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();

            // Assert
            Assert.Equal(
                fakeWritePhysicalQueue,
                this.monitor.WriteShard
            );
        }

        /// <summary>
        /// Verifies that the WriteShard property reflects the current write shard.
        /// This scenario simulates the situation where the current write shard
        /// just crosses the scale up threshold and a new write shard is added.
        /// Once the new write shard is added, the old is disabled.
        /// During this time there will be a short period where the old and new
        /// write shards exist simultaneously since these operations cannot be
        /// completed as a single atomic operation.
        /// </summary>
        /// <returns>Returns the async Task for the test.</returns>
        [Fact]
        public
        async Task
        WriteShard_MultipleWriteShardsExists_LastSlinqyQueueWriteShardIsReturned()
        {
            // Arrange
            var fakeReadPhysicalQueue     = A.Fake<SlinqyQueueShard>();
            var fakeOldWritePhysicalQueue = A.Fake<SlinqyQueueShard>();
            var fakeNewWritePhysicalQueue = A.Fake<SlinqyQueueShard>();

            A.CallTo(() => fakeReadPhysicalQueue.Writable).Returns(false);
            A.CallTo(() => fakeOldWritePhysicalQueue.Writable).Returns(true);
            A.CallTo(() => fakeNewWritePhysicalQueue.Writable).Returns(true);

            var physicalQueues = new List<SlinqyQueueShard> {
                fakeReadPhysicalQueue,
                fakeOldWritePhysicalQueue,
                fakeNewWritePhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();

            // Assert
            Assert.Equal(
                fakeNewWritePhysicalQueue,
                this.monitor.WriteShard
            );
        }
    }
}