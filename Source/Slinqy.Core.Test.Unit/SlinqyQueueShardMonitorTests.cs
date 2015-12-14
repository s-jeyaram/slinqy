namespace Slinqy.Core.Test.Unit
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    /// <summary>
    /// Tests functionality exposed by the SlinqyQueueShardMonitor class.
    /// </summary>
    public class SlinqyQueueShardMonitorTests
    {
        /// <summary>
        /// Represents a valid value where one is needed for a Slinqy queue name parameters.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const string ValidSlinqyQueueName = "queue-name";

        /// <summary>
        /// The fake that simulates a physical queue service.
        /// </summary>
        private readonly IPhysicalQueueService fakeQueueService = A.Fake<IPhysicalQueueService>();

        /// <summary>
        /// The instance under test.
        /// </summary>
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
            var fakePhysicalQueue1 = A.Fake<IPhysicalQueue>();
            var fakePhysicalQueue2 = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakePhysicalQueue1.Name).Returns(ValidSlinqyQueueName + "1");
            A.CallTo(() => fakePhysicalQueue2.Name).Returns(ValidSlinqyQueueName + "2");

            var physicalQueues = new List<IPhysicalQueue> {
                fakePhysicalQueue1,
                fakePhysicalQueue2
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();                  // Start monitoring for shards.

            var slinqyQueueShards = this.monitor.Shards; // Retrieve shards that were detected.

            // Assert
            Assert.Equal(
                physicalQueues.First().MaxSizeMegabytes,
                slinqyQueueShards.First().PhysicalQueue.MaxSizeMegabytes
            );
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
            var fakeReadWritePhysicalQueue = A.Fake<IPhysicalQueue>();

            A.CallTo(() =>
                fakeReadWritePhysicalQueue.Name
            ).Returns(ValidSlinqyQueueName + "0");

            A.CallTo(() =>
                fakeReadWritePhysicalQueue.IsSendEnabled
            ).Returns(true);

            var physicalQueues = new List<IPhysicalQueue> {
                fakeReadWritePhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();

            // Assert
            Assert.Equal(
                fakeReadWritePhysicalQueue.Name,
                this.monitor.SendShard.PhysicalQueue.Name
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
            var fakeReadPhysicalQueue  = A.Fake<IPhysicalQueue>();
            var fakeWritePhysicalQueue = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakeReadPhysicalQueue.Name).Returns("shard0");
            A.CallTo(() => fakeReadPhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeWritePhysicalQueue.Name).Returns("shard1");
            A.CallTo(() => fakeWritePhysicalQueue.IsSendEnabled).Returns(true);

            var physicalQueues = new List<IPhysicalQueue> {
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
                fakeWritePhysicalQueue.Name,
                this.monitor.SendShard.PhysicalQueue.Name
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
            var fakeReadPhysicalQueue      = A.Fake<IPhysicalQueue>();
            var fakeDisabled1PhysicalQueue = A.Fake<IPhysicalQueue>();
            var fakeDisabled2PhysicalQueue = A.Fake<IPhysicalQueue>();
            var fakeDisabled3PhysicalQueue = A.Fake<IPhysicalQueue>();
            var fakeWritePhysicalQueue     = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakeReadPhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeReadPhysicalQueue.Name).Returns("shard0");
            A.CallTo(() => fakeDisabled1PhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeDisabled1PhysicalQueue.Name).Returns("shard1");
            A.CallTo(() => fakeDisabled2PhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeDisabled2PhysicalQueue.Name).Returns("shard2");
            A.CallTo(() => fakeDisabled3PhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeDisabled3PhysicalQueue.Name).Returns("shard3");
            A.CallTo(() => fakeWritePhysicalQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeWritePhysicalQueue.Name).Returns("shard4");

            var physicalQueues = new List<IPhysicalQueue> {
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
                fakeWritePhysicalQueue.Name,
                this.monitor.SendShard.PhysicalQueue.Name
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
            var fakeReadPhysicalQueue     = A.Fake<IPhysicalQueue>();
            var fakeOldWritePhysicalQueue = A.Fake<IPhysicalQueue>();
            var fakeNewWritePhysicalQueue = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakeReadPhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeReadPhysicalQueue.Name).Returns("shard0");
            A.CallTo(() => fakeOldWritePhysicalQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeOldWritePhysicalQueue.Name).Returns("shard1");
            A.CallTo(() => fakeNewWritePhysicalQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeNewWritePhysicalQueue.Name).Returns("shard2");

            var physicalQueues = new List<IPhysicalQueue> {
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
                fakeNewWritePhysicalQueue.Name,
                this.monitor.SendShard.PhysicalQueue.Name
            );
        }
    }
}