namespace Slinqy.Core.Test.Unit
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// Tests functions of the SlinqyQueue class.
    /// </summary>
    public static class SlinqyQueueTests
    {
        /// <summary>
        /// Verifies that the MaxQueueSizeMegabytes property returns a sum of all the capacities of its shards.
        /// </summary>
        [Fact]
        public
        static
        void
        MaxQueueSizeMegabytes_Always_ReturnsSumOfAllShardSizes()
        {
            // Arrange
            var fakeShards = new List<SlinqyQueueShard> {
                new SlinqyQueueShard("test-01", 1024, 0),
                new SlinqyQueueShard("test-02", 1024, 0)
            };

            using (var slinqyQueue = new SlinqyQueue("test", queueName => Task.Run(() => fakeShards.AsEnumerable())))
            {
                // Act
                var actual = slinqyQueue.MaxQueueSizeMegabytes;

                // Assert
                Assert.Equal(2048, actual);
            }
        }
    }
}
