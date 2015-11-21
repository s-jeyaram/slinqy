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
    }
}