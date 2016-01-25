namespace Slinqy.Core.Test.Unit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
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
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
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
        /// Verifies that the SendShard property reflects the current send shard.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        WriteShard_OneSendReceiveShardExists_SlinqyQueueSendShardIsReturned()
        {
            // Arrange
            var fakeSendReceivePhysicalQueue = A.Fake<IPhysicalQueue>();

            A.CallTo(() =>
                fakeSendReceivePhysicalQueue.Name
            ).Returns(ValidSlinqyQueueName + "0");

            A.CallTo(() =>
                fakeSendReceivePhysicalQueue.IsSendEnabled
            ).Returns(true);

            var physicalQueues = new List<IPhysicalQueue> {
                fakeSendReceivePhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();

            // Assert
            Assert.Equal(
                fakeSendReceivePhysicalQueue.Name,
                this.monitor.SendShard.PhysicalQueue.Name
            );
        }

        /// <summary>
        /// Verifies that the WriteShard property reflects the current write shard.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        WriteShard_OneReceiveOneSendShardExists_SlinqyQueueSendShardIsReturned()
        {
            // Arrange
            var fakeReceivePhysicalQueue  = A.Fake<IPhysicalQueue>();
            var fakeSendPhysicalQueue     = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakeReceivePhysicalQueue.Name).Returns("shard0");
            A.CallTo(() => fakeReceivePhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeSendPhysicalQueue.Name).Returns("shard1");
            A.CallTo(() => fakeSendPhysicalQueue.IsSendEnabled).Returns(true);

            var physicalQueues = new List<IPhysicalQueue> {
                fakeReceivePhysicalQueue,
                fakeSendPhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();

            // Assert
            Assert.Equal(
                fakeSendPhysicalQueue.Name,
                this.monitor.SendShard.PhysicalQueue.Name
            );
        }

        /// <summary>
        /// Verifies that the SendShard property reflects the current send shard.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SendShard_OneReceiveManyDisabledOneSendShardExists_SlinqyQueueSendShardIsReturned()
        {
            // Arrange
            var fakeReceivePhysicalQueue    = A.Fake<IPhysicalQueue>();
            var fakeDisabled1PhysicalQueue  = A.Fake<IPhysicalQueue>();
            var fakeDisabled2PhysicalQueue  = A.Fake<IPhysicalQueue>();
            var fakeDisabled3PhysicalQueue  = A.Fake<IPhysicalQueue>();
            var fakeSendPhysicalQueue       = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakeReceivePhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeReceivePhysicalQueue.Name).Returns("shard0");
            A.CallTo(() => fakeDisabled1PhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeDisabled1PhysicalQueue.Name).Returns("shard1");
            A.CallTo(() => fakeDisabled2PhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeDisabled2PhysicalQueue.Name).Returns("shard2");
            A.CallTo(() => fakeDisabled3PhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeDisabled3PhysicalQueue.Name).Returns("shard3");
            A.CallTo(() => fakeSendPhysicalQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeSendPhysicalQueue.Name).Returns("shard4");

            var physicalQueues = new List<IPhysicalQueue> {
                fakeReceivePhysicalQueue,
                fakeSendPhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();

            // Assert
            Assert.Equal(
                fakeSendPhysicalQueue.Name,
                this.monitor.SendShard.PhysicalQueue.Name
            );
        }

        /// <summary>
        /// Verifies that the SendShard property reflects the current send shard.
        /// This scenario simulates the situation where the current send shard
        /// just crosses the scale up threshold and a new send shard is added.
        /// Once the new send shard is added, the old is disabled.
        /// During this time there will be a short period where the old and new
        /// send shards exist simultaneously since these operations cannot be
        /// completed as a single atomic operation.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SendShard_MultipleSendShardsExists_LastSlinqyQueueSendShardIsReturned()
        {
            // Arrange
            var fakeReceivePhysicalQueue    = A.Fake<IPhysicalQueue>();
            var fakeOldSendPhysicalQueue    = A.Fake<IPhysicalQueue>();
            var fakeNewSendPhysicalQueue    = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakeReceivePhysicalQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeReceivePhysicalQueue.Name).Returns("shard0");
            A.CallTo(() => fakeOldSendPhysicalQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeOldSendPhysicalQueue.Name).Returns("shard1");
            A.CallTo(() => fakeNewSendPhysicalQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeNewSendPhysicalQueue.Name).Returns("shard2");

            var physicalQueues = new List<IPhysicalQueue> {
                fakeReceivePhysicalQueue,
                fakeOldSendPhysicalQueue,
                fakeNewSendPhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();

            // Assert
            Assert.Equal(
                fakeNewSendPhysicalQueue.Name,
                this.monitor.SendShard.PhysicalQueue.Name
            );
        }

        /// <summary>
        /// Verifies that the ReceiveShard property reflects the current receive shard.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        ReceiveShard_OneSendReceiveShardExists_SlinqyQueueReceiveShardIsReturned()
        {
            // Arrange
            var fakeSendReceivePhysicalQueue = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakeSendReceivePhysicalQueue.Name).Returns(ValidSlinqyQueueName + "0");
            A.CallTo(() => fakeSendReceivePhysicalQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeSendReceivePhysicalQueue.IsReceiveEnabled).Returns(true);

            var physicalQueues = new List<IPhysicalQueue> {
                fakeSendReceivePhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            await this.monitor.Start();

            // Act
            var actualQueueShard = this.monitor.ReceiveShard;

            // Assert
            Assert.Equal(fakeSendReceivePhysicalQueue, actualQueueShard.PhysicalQueue);
        }

        /// <summary>
        /// Verifies that the SlinqyQueueShardMonitor does not end if an unhandled exception occurs
        /// while monitoring the physical queues, but instead retries the operation.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyQueueShardMonitor_ExceptionOccursDuringRefresh_Retries()
        {
            // Arrange
            await this.monitor.Start();

            A.CallTo(() => this.fakeQueueService.ListQueues(A<string>.Ignored))
                .Throws<Exception>()
                .Once();

            // Act
            Thread.Sleep(2000); // TODO: Reduce when monitor poll delay is configurable.

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.ListQueues(A<string>.Ignored)
            ).MustHaveHappened(Repeated.AtLeast.Twice);
        }

        /// <summary>
        /// Verifies that the SlinqyQueueMonitor stops polling the queuing service after Stop is called.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        StopMonitoring_IsRunning_StopsPollingTheQueuingService()
        {
            // Arrange
            await this.monitor.Start();

            // Act
            this.monitor.StopMonitoring();
            Thread.Sleep(2000); // TODO: Reduce when monitor delay is configurable.

            // Assert
            A.CallTo(
                () => this.fakeQueueService.ListQueues(A<string>.Ignored)
            ).MustHaveHappened(Repeated.Exactly.Twice);
        }

        /// <summary>
        /// Verifies that the Slinqy Agent Queue doesn't make its way to the monitors list of queue shards.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyQueueShardMonitor_AgentQueueExistsInPhysicalQueueList_DoesNotAppearInShardsProperty()
        {
            // Arrange
            var fakeSendReceivePhysicalQueue = A.Fake<IPhysicalQueue>();
            var fakeSlinqyAgentPhysicalQueue = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakeSendReceivePhysicalQueue.Name).Returns(ValidSlinqyQueueName + "0");
            A.CallTo(() => fakeSendReceivePhysicalQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeSendReceivePhysicalQueue.IsReceiveEnabled).Returns(true);
            A.CallTo(() => fakeSlinqyAgentPhysicalQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeSlinqyAgentPhysicalQueue.IsReceiveEnabled).Returns(true);
            A.CallTo(() => fakeSlinqyAgentPhysicalQueue.Name).Returns(ValidSlinqyQueueName + "-agent");

            var physicalQueues = new List<IPhysicalQueue> {
                fakeSlinqyAgentPhysicalQueue,
                fakeSendReceivePhysicalQueue
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(physicalQueues);

            // Act
            await this.monitor.Start();

            // Assert
            Assert.False(this.monitor.Shards.Any(s => s.PhysicalQueue.Name.Contains("-agent")));
        }

        /// <summary>
        /// Verifies that SendShard returns null when there are no send shards.
        /// </summary>
        [Fact]
        public
        void
        SendShard_NoSendEnabledPhysicalQueues_ReturnsNull()
        {
            // Arrange
            A.CallTo(() =>
                this.fakeQueueService.ListQueues(A<string>.Ignored)
            ).Returns(Enumerable.Empty<IPhysicalQueue>());

            // Act
            var actual = this.monitor.SendShard;

            // Assert
            Assert.Null(actual);
        }
    }
}