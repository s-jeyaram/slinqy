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
        /// Represents a valid value where one is needed for a Slinqy Agent name.
        /// Should not be a special value other than it is guaranteed to be valid.
        /// </summary>
        private const string ValidSlinqyAgentName = ValidSlinqyQueueName + SlinqyAgent.AgentQueueNameSuffix;

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
        /// A fake SlinqyQueueShard that is configured to be writable.
        /// </summary>
        private readonly SlinqyQueueShard fakeShard;

        /// <summary>
        /// A fake SlinqyQueueShardMonitor.
        /// </summary>
        private readonly SlinqyQueueShardMonitor fakeQueueShardMonitor;

        /// <summary>
        /// The list of fake shards that the fakeQueueShardMonitor works from.
        /// </summary>
        private readonly List<SlinqyQueueShard> fakeShards;

        /// <summary>
        /// A fake IPhysicalQueue that represents the agents queue.
        /// </summary>
        private IPhysicalQueue fakeAgentQueue;

        /// <summary>
        /// Maintains a count of the number of shards that have been created to simplify generating of shard indexes and names.
        /// </summary>
        private int shardIndexCounter;

        /// <summary>
        /// A reference to the function internal to the SlinqyAgent that handles new messages so that the unit tests can invoke is as if it was received.
        /// </summary>
        private Func<EvaluateShardsCommand, Task> agentEvaluateShardsCommandHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyAgentTests"/> class with default/common behaviors and values.
        /// </summary>
        public
        SlinqyAgentTests()
        {
            // Configures one read/writable shard since that's the minimum valid state.
            this.fakeShard = this.CreateFakeSendReceiveQueue();

            // Configures the fake shard monitor to use the fake shards.
            this.fakeQueueShardMonitor = A.Fake<SlinqyQueueShardMonitor>();
            this.fakeShards = new List<SlinqyQueueShard> { this.fakeShard };

            // Configures the fake agent queue to use.
            this.fakeAgentQueue = CreateFakeAgentQueue();

            A.CallTo(() => this.fakeQueueService.CreateQueue(ValidSlinqyAgentName)).Returns(this.fakeAgentQueue);
            A.CallTo(() => this.fakeAgentQueue.OnReceive(A<Func<EvaluateShardsCommand, Task>>.Ignored)).Invokes(call =>
            {
                this.agentEvaluateShardsCommandHandler = call.GetArgument<Func<EvaluateShardsCommand, Task>>(0);
            });

            A.CallTo(() => this.fakeQueueShardMonitor.SendShard).Returns(this.fakeShard);
            A.CallTo(() => this.fakeQueueShardMonitor.QueueName).Returns(ValidSlinqyQueueName);
            A.CallTo(() => this.fakeQueueShardMonitor.Shards).Returns(this.fakeShards);

            // Configure the agent that will be tested.
            this.slinqyAgent = new SlinqyAgent(
                queueService:                       this.fakeQueueService,
                slinqyQueueShardMonitor:            this.fakeQueueShardMonitor,
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
        SlinqyAgent_WriteQueueShardExceedsCapacityThreshold_ASendOnlyShardIsAdded()
        {
            // Arrange
            A.CallTo(() => this.fakeShard.StorageUtilization).Returns(ValidStorageCapacityScaleOutThreshold + 0.01);
            await this.slinqyAgent.Start();

            // Act
            await this.agentEvaluateShardsCommandHandler(new EvaluateShardsCommand());

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateSendOnlyQueue(A<string>.Ignored)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the agent doesn't add shards when the send
        /// queues size increases but remains under the scale up threshold.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_SendQueueShardSizeUnderCapacityThreshold_AnotherShardIsNotAdded()
        {
            // Arrange
            A.CallTo(() =>
                this.fakeShard.StorageUtilization
            ).Returns(ValidStorageCapacityScaleOutThreshold - 0.01);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            // Basically says: A call to CreateQueue must not have happened unless the name is ValidSlinqyAgentName.
            A.CallTo(() =>
                this.fakeQueueService.CreateQueue(A<string>.That.Matches(name => name != ValidSlinqyAgentName))
            ).MustNotHaveHappened();
        }

        /// <summary>
        /// Verifies that the previous shard is set to be receive only after a new write shard is added.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_SecondShardAdded_FirstShardIsSetToReceiveOnly()
        {
            // Arrange
            // Configure the write shards size to trigger scaling.
            A.CallTo(() =>
                this.fakeShard.StorageUtilization
            ).Returns(ValidStorageCapacityScaleOutThreshold + 0.01);

            // Configure the new shard that will be added as a result of the scaling.
            var fakeAdditionalShard = this.CreateFakeSendOnlyQueue();

            A.CallTo(() => this.fakeQueueService.CreateSendOnlyQueue(A<string>.Ignored))
                .Invokes(() => this.fakeShards.Add(fakeAdditionalShard))
                .Returns(fakeAdditionalShard.PhysicalQueue);

            await this.slinqyAgent.Start();

            // Act
            await this.agentEvaluateShardsCommandHandler(new EvaluateShardsCommand());

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueReceiveOnly(this.fakeShard.PhysicalQueue.Name)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that shards existing in between the send and receive shard are disabled.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_ThirdShardIsAdded_MiddleShardIsDisabled()
        {
            // Arrange
            var scaleOutSizeMegabytes = Math.Ceiling(ValidMaxSizeMegabytes * ValidStorageCapacityScaleOutThreshold);
            var scaleOutSizeBytes     = Convert.ToInt64(scaleOutSizeMegabytes * 1024 * 1024);

            // Configure the first shard to be receive-only.
            var fakeReceiveOnlyShard = this.CreateFakeReceiveOnlyQueue();

            // Create a new send-only shard that has exceeded the scale out threshold,
            // this will become the middle shard after scaling occurs.
            var fakeSendOnlyShard = this.CreateFakeSendOnlyQueue(currentSizeBytes: scaleOutSizeBytes);

            A.CallTo(() => this.fakeQueueShardMonitor.SendShard).Returns(fakeSendOnlyShard);
            A.CallTo(() => this.fakeQueueShardMonitor.ReceiveShard).Returns(fakeReceiveOnlyShard);

            this.fakeShards.Clear();
            this.fakeShards.Add(fakeReceiveOnlyShard);
            this.fakeShards.Add(fakeSendOnlyShard);

            // Configure the new shard that will be added as a result of the scaling.
            var newSendOnlyShard = this.CreateFakeSendOnlyQueue();

            A.CallTo(() => this.fakeQueueService.CreateSendOnlyQueue(A<string>.Ignored))
                .Invokes(() => this.fakeShards.Add(newSendOnlyShard))
                .Returns(newSendOnlyShard.PhysicalQueue);

            await this.slinqyAgent.Start();

            // Act
            await this.agentEvaluateShardsCommandHandler(new EvaluateShardsCommand());

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueDisabled(fakeSendOnlyShard.PhysicalQueue.Name)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that once a shard has been disabled the agent doesn't continually try to disable it.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_ShardsAlreadyDisabled_DoesNotDisableAgain()
        {
            // Arrange
            // Configure the first shard to be receive-only.
            var fakeReceiveOnlyShard    = this.CreateFakeReceiveOnlyQueue();
            var fakeDisabledMiddleShard = this.CreateFakeDisabledQueue();
            var fakeSendOnlyShard       = this.CreateFakeSendOnlyQueue();

            this.fakeShards.Clear();
            this.fakeShards.Add(fakeReceiveOnlyShard);
            this.fakeShards.Add(fakeDisabledMiddleShard);
            this.fakeShards.Add(fakeSendOnlyShard);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueDisabled(A<string>.Ignored)
            ).MustNotHaveHappened();
        }

        /// <summary>
        /// Verifies that if a send-able/receivable shard is found to be send only, it will be set to active.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_SendReceivableShardIsSendOnly_IsSetToEnabled()
        {
            // Arrange
            A.CallTo(() => this.fakeShard.IsReceiveOnly).Returns(false);
            await this.slinqyAgent.Start();

            // Act
            await this.agentEvaluateShardsCommandHandler(new EvaluateShardsCommand());

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.SetQueueEnabled(A<string>.Ignored)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the agent stops polling the SlinqyQueueMonitor after Stop is called.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        Stop_IsRunning_StopsPollingTheMonitor()
        {
            // Arrange
            await this.slinqyAgent.Start();

            // Act
            this.slinqyAgent.Stop();
            Thread.Sleep(2000); // TODO: Reduce when monitor delay is configurable.

            // Assert
            A.CallTo(
                () => this.fakeQueueShardMonitor.Shards
            ).MustHaveHappened(Repeated.Exactly.Once);
        }

        /// <summary>
        /// Verifies that the agent stops the monitor when Stop is called.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        Stop_IsRunning_StopsTheMonitor()
        {
            // Arrange
            await this.slinqyAgent.Start();

            // Act
            this.slinqyAgent.Stop();

            // Assert
            A.CallTo(
                () => this.fakeQueueShardMonitor.StopMonitoring()
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the SlinqyAgent creates it's own queue (for polling purposes) if it doesn't already exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        Start_AgentQueueDoesNotExist_AgentQueueIsCreated()
        {
            // Arrange
            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyAgentName)
            ).Returns(Enumerable.Empty<IPhysicalQueue>());

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateQueue(ValidSlinqyAgentName)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the SlinqyAgent enqueues the polling message in its agent queue after it is created.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        Start_AgentQueueIsCreated_EvaluateShardsCommandIsSent()
        {
            // Arrange
            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyAgentName)
            ).Returns(Enumerable.Empty<IPhysicalQueue>());

            var fakeAgentQueue = CreateFakeAgentQueue();

            A.CallTo(() =>
                this.fakeQueueService.CreateQueue(ValidSlinqyAgentName)
            ).Returns(fakeAgentQueue);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                fakeAgentQueue.Send(A<object>.That.IsInstanceOf(typeof(EvaluateShardsCommand)), A<DateTimeOffset>.Ignored)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the SlinqyAgent starts receiving its agent queue.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        Start_Always_StartsReceivingAgentQueue()
        {
            // Arrange
            var fakeAgentQueue = CreateFakeAgentQueue();

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyAgentName)
            ).Returns(new[] { fakeAgentQueue });

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                fakeAgentQueue.OnReceive(A<Func<EvaluateShardsCommand, Task>>.Ignored)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the SlinqyAgent doesn't try to create it's own queue if it already exists.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        Start_AgentQueueExists_DoesNotTryToCreateAgain()
        {
            // Arrange
            var fakePhysicalQueues = new List<IPhysicalQueue> {
                CreateFakeAgentQueue()
            };

            A.CallTo(() =>
                this.fakeQueueService.ListQueues(ValidSlinqyAgentName)
            ).Returns(fakePhysicalQueues);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateQueue(ValidSlinqyAgentName)
            ).MustNotHaveHappened();
        }

        /// <summary>
        /// Verifies that a new EvaluateShardsCommand is enqueued after successfully processing the current one (but before it is deleted from the queue).
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        SlinqyAgent_EvaluateShardsCommandIsProcessed_NewEvaluateShardsCommandIsScheduled()
        {
            // Arrange
            await this.slinqyAgent.Start();

            // Act
            await this.agentEvaluateShardsCommandHandler(new EvaluateShardsCommand());

            // Assert
            A.CallTo(() =>
                this.fakeAgentQueue.Send(A<EvaluateShardsCommand>.That.Not.IsNull(), A<DateTimeOffset>.Ignored)
            ).MustHaveHappened(Repeated.Exactly.Twice);
        }

        /// <summary>
        /// Verifies that the first shard is created if it doesn't exist.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
        [Fact]
        public
        async Task
        Start_SendShardDoesNotExist_SendShardIsCreated()
        {
            // Arrange
            A.CallTo(() => this.fakeQueueShardMonitor.SendShard).Returns(null);

            // Act
            await this.slinqyAgent.Start();

            // Assert
            A.CallTo(() =>
                this.fakeQueueService.CreateQueue(ValidSlinqyQueueName + "0")
            ).MustHaveHappened();
        }

        /// <summary>
        /// Create a new fake IPhysicalQueue configured as a Slinqy Agent queue.
        /// </summary>
        /// <returns>Returns the fake queue.</returns>
        private
        static
        IPhysicalQueue
        CreateFakeAgentQueue()
        {
            var fakeAgentQueue = A.Fake<IPhysicalQueue>();

            A.CallTo(() => fakeAgentQueue.Name).Returns(ValidSlinqyAgentName);

            return fakeAgentQueue;
        }

        /// <summary>
        /// Creates a new instance of SlinqyQueueShard as a fake that is configured to simulate a send/receive queue.
        /// </summary>
        /// <returns>Returns a fake that simulates a send/receive queue.</returns>
        private
        SlinqyQueueShard
        CreateFakeSendReceiveQueue()
        {
            return this.CreateFakeShard(sendable: true, receivable: true);
        }

        /// <summary>
        /// Creates a new instance of SlinqyQueueShard as a fake that is configured to simulate a send-only queue.
        /// </summary>
        /// <param name="currentSizeBytes">Specifies the current size of the fake shard.</param>
        /// <returns>
        /// Returns a fake that simulates a new queue shard.
        /// </returns>
        private
        SlinqyQueueShard
        CreateFakeSendOnlyQueue(
            long currentSizeBytes = 0)
        {
            return this.CreateFakeShard(
                sendable:           true,
                receivable:         false,
                currentSizeBytes:   currentSizeBytes
            );
        }

        /// <summary>
        /// Creates a new instance of SlinqyQueueShard as a fake that is configured to simulate a receive-only queue.
        /// </summary>
        /// <param name="currentSizeBytes">Specifies the current size of the fake shard.</param>
        /// <returns>
        /// Returns a fake that simulates a new queue shard.
        /// </returns>
        private
        SlinqyQueueShard
        CreateFakeReceiveOnlyQueue(
            long currentSizeBytes = 1)
        {
            return this.CreateFakeShard(
                sendable:           false,
                receivable:         true,
                currentSizeBytes:   currentSizeBytes
            );
        }

        /// <summary>
        /// Creates a new instance of SlinqyQueueShard as a fake that is configured to simulate a disabled queue.
        /// </summary>
        /// <returns>
        /// Returns a fake that simulates a new queue shard.
        /// </returns>
        private
        SlinqyQueueShard
        CreateFakeDisabledQueue()
        {
            return this.CreateFakeShard(sendable: false, receivable: false);
        }

        /// <summary>
        /// Creates a new fake SlinqyQueueShard.
        /// </summary>
        /// <param name="sendable">Specifies if the fake shard can be sent to.</param>
        /// <param name="receivable">Specifies if the fake shard can be received from.</param>
        /// <param name="maxSizeMegabytes">Specifies the max size of the fake shard.</param>
        /// <param name="currentSizeBytes">Specifies the current size of the fake shard.</param>
        /// <returns>Returns the fake shard.</returns>
        private
        SlinqyQueueShard
        CreateFakeShard(
            bool sendable           = true,
            bool receivable         = true,
            long maxSizeMegabytes   = ValidMaxSizeMegabytes,
            long currentSizeBytes   = 0)
        {
            var fakeShard           = A.Fake<SlinqyQueueShard>();
            var fakePhysicalQueue   = fakeShard.PhysicalQueue;

            A.CallTo(() => fakeShard.ShardIndex                 ).Returns(this.shardIndexCounter++);
            A.CallTo(() => fakeShard.IsDisabled                 ).Returns(!sendable && !receivable);
            A.CallTo(() => fakeShard.IsReceiveOnly              ).Returns(!sendable && receivable);
            A.CallTo(() => fakeShard.StorageUtilization         ).Returns(currentSizeBytes == 0 ? 0 : 1);
            A.CallTo(() => fakePhysicalQueue.Name               ).Returns(ValidSlinqyQueueName + fakeShard.ShardIndex);
            A.CallTo(() => fakePhysicalQueue.IsSendEnabled      ).Returns(sendable);
            A.CallTo(() => fakePhysicalQueue.IsReceiveEnabled   ).Returns(receivable);
            A.CallTo(() => fakePhysicalQueue.MaxSizeMegabytes   ).Returns(maxSizeMegabytes);
            A.CallTo(() => fakePhysicalQueue.CurrentSizeBytes   ).Returns(currentSizeBytes);

            return fakeShard;
        }
    }
}