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
        /// The list of physical queues that the fake IPhysicalQueueService has.
        /// </summary>
        private readonly List<IPhysicalQueue> fakeQueueServicePhysicalQueues = new List<IPhysicalQueue>();

        /// <summary>
        /// The instance under test.
        /// </summary>
        private readonly SlinqyQueueShardMonitor monitor;

        /// <summary>
        /// Keeps count of the number of fake physical queues that have been created.
        /// </summary>
        private int queueCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueShardMonitorTests"/> class.
        /// </summary>
        public
        SlinqyQueueShardMonitorTests()
        {
            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyQueueName)
            ).Returns(this.fakeQueueServicePhysicalQueues);

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
            this.AddNewReceiveOnlyQueue();
            this.AddNewSendOnlyQueue();

            // Act
            await this.monitor.Start();                  // Start monitoring for shards.

            var slinqyQueueShards = this.monitor.Shards; // Retrieve shards that were detected.

            // Assert
            Assert.Equal(
                this.fakeQueueServicePhysicalQueues.First().MaxSizeMegabytes,
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
        SendShard_OneSendReceiveShardExists_SlinqyQueueSendShardIsReturned()
        {
            // Arrange
            var fakeSendReceivePhysicalQueue = this.AddNewSendReceiveQueue();

            // Act
            await this.monitor.Start();

            // Assert
            Assert.Equal(
                fakeSendReceivePhysicalQueue.Name,
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
        SendShard_OneReceiveOneSendShardExists_SlinqyQueueSendShardIsReturned()
        {
            // Arrange
            this.AddNewReceiveOnlyQueue();
            var fakeSendPhysicalQueue = this.AddNewSendOnlyQueue();

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
            this.AddNewReceiveOnlyQueue();
            this.AddNewDisabledQueue();
            this.AddNewDisabledQueue();
            this.AddNewDisabledQueue();
            var fakeSendPhysicalQueue = this.AddNewSendOnlyQueue();

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
            this.AddNewReceiveOnlyQueue();
            this.AddNewSendOnlyQueue();
            var fakeNewSendPhysicalQueue = this.AddNewSendOnlyQueue();

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
            var fakeSendReceivePhysicalQueue = this.AddNewSendReceiveQueue();

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

        /// <summary>
        /// Creates a new fake IPhysicalQueue and adds it to the fake IPhysicalQueueServices list of queues.
        /// </summary>
        /// <returns>Returns the instance that was created.</returns>
        private
        IPhysicalQueue
        AddNewSendReceiveQueue()
        {
            var fakeQueue = this.CreateFakePhysicalQueue();

            A.CallTo(() => fakeQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeQueue.IsReceiveEnabled).Returns(true);

            this.fakeQueueServicePhysicalQueues.Add(fakeQueue);

            return fakeQueue;
        }

        /// <summary>
        /// Creates a new fake IPhysicalQueue and adds it to the fake IPhysicalQueueServices list of queues.
        /// </summary>
        /// <returns>Returns the instance that was created.</returns>
        private
        IPhysicalQueue
        AddNewReceiveOnlyQueue()
        {
            var fakeQueue = this.CreateFakePhysicalQueue();

            A.CallTo(() => fakeQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeQueue.IsReceiveEnabled).Returns(true);

            this.fakeQueueServicePhysicalQueues.Add(fakeQueue);

            return fakeQueue;
        }

        /// <summary>
        /// Creates a new fake IPhysicalQueue and adds it to the fake IPhysicalQueueServices list of queues.
        /// </summary>
        /// <returns>Returns the instance that was created.</returns>
        private
        IPhysicalQueue
        AddNewSendOnlyQueue()
        {
            var fakeQueue = this.CreateFakePhysicalQueue();

            A.CallTo(() => fakeQueue.IsSendEnabled).Returns(true);
            A.CallTo(() => fakeQueue.IsReceiveEnabled).Returns(false);

            this.fakeQueueServicePhysicalQueues.Add(fakeQueue);

            return fakeQueue;
        }

        /// <summary>
        /// Creates a new fake IPhysicalQueue and adds it to the fake IPhysicalQueueServices list of queues.
        /// </summary>
        /// <returns>Returns the instance that was created.</returns>
        private
        IPhysicalQueue
        AddNewDisabledQueue()
        {
            var fakeQueue = this.CreateFakePhysicalQueue();

            A.CallTo(() => fakeQueue.IsSendEnabled).Returns(false);
            A.CallTo(() => fakeQueue.IsReceiveEnabled).Returns(false);

            this.fakeQueueServicePhysicalQueues.Add(fakeQueue);

            return fakeQueue;
        }

        /// <summary>
        /// Create a new fake instance of the IPhysicalQueue interface for testing.
        /// </summary>
        /// <returns>Returns a new fake instance.</returns>
        private
        IPhysicalQueue
        CreateFakePhysicalQueue()
        {
            var fakePhysicalQueue = A.Fake<IPhysicalQueue>();

            A.CallTo(() =>
                fakePhysicalQueue.Name
            ).Returns(ValidSlinqyQueueName + this.queueCount++);

            return fakePhysicalQueue;
        }
    }
}