namespace Slinqy.Core.Test.Unit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    /// <summary>
    /// Tests functionality of the SlinqyQueueShard class.
    /// </summary>
    public class SlinqyQueueShardTests
    {
        /// <summary>
        /// Verifies that the SendBatch method properly submits the batch to the underlying physical queue.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public
        async Task
        SendBatch_BatchIsValid_BatchSentToPhysicalQueue()
        {
            // Arrange
            var fakePhysicalQueue = A.Fake<IPhysicalQueue>();
            var queueShard        = new SlinqyQueueShard(fakePhysicalQueue);
            var validBatch        = new List<string> { "message 1", "message 2" };

            // Act
            await queueShard.SendBatch(validBatch);

            // Assert
            A.CallTo(() =>
                fakePhysicalQueue.SendBatch(A<IEnumerable<object>>.Ignored)
            ).MustHaveHappened();
        }
    }
}
