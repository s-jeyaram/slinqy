namespace Slinqy.Core.Test.Unit
{
    using System.Collections.Generic;
    using FakeItEasy;
    using Xunit;

    /// <summary>
    /// Tests functions of the SlinqyQueue class.
    /// </summary>
    public class SlinqyQueueTests
    {
        private readonly IPhysicalQueueService fakePhysicalQueueService = A.Fake<IPhysicalQueueService>();

        /// <summary>
        /// Verifies that the MaxQueueSizeMegabytes property returns a sum of all the capacities of its shards.
        /// </summary>
        [Fact]
        public
        void
        MaxQueueSizeMegabytes_Always_ReturnsSumOfAllShardSizes()
        {
            // Arrange
            var fakeShards = new List<SlinqyQueueShard> {
                new SlinqyQueueShard("test-01", 0, 1024, 1024, false),
                new SlinqyQueueShard("test-02", 1, 1024, 0, true)
            };

            A.CallTo(() =>
                this.fakePhysicalQueueService.ListQueues(A<string>.Ignored)
            ).Returns(fakeShards);

            var slinqyQueue = new SlinqyQueue("test", this.fakePhysicalQueueService);

            // Act
            var actual = slinqyQueue.MaxQueueSizeMegabytes;

            // Assert
            Assert.Equal(2048, actual);
        }
    }
}
