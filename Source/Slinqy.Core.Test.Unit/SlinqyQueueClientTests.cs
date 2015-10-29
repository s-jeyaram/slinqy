namespace Slinqy.Core.Test.Unit
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// Tests functions of the SlinqyQueueClient class.
    /// </summary>
    public static class SlinqyQueueClientTests
    {
        /// <summary>
        /// Verifies the constructor checks for null values.
        /// </summary>
        [Fact]
        public
        static
        void
        Constructor_CreatePhysicalQueueDelegateIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SlinqyQueueClient(null));
        }

        /// <summary>
        /// Verifies that the create delegate is called when CreateAsync is called.
        /// </summary>
        /// <returns>Returns the async Task.</returns>
        [Fact]
        public
        static
        async Task
        CreateAsync_QueueNameValid_CreateDelegateInvoked()
        {
            // Arrange
            var delegateCalled = false;

            var client = new SlinqyQueueClient(
                createPhysicalQueueDelegate: queueName =>
                {
                    delegateCalled = true;
                    return Task.Run(() => new SlinqyQueue(queueName, 1));
                }

            );

            // Act
            await client.CreateAsync("test");

            // Assert
            Assert.True(delegateCalled);
        }

        /// <summary>
        /// Verifies that the queue name is valid.
        /// </summary>
        /// <returns>Returns the async Task.</returns>
        [Fact]
        public
        static
        async Task
        CreateAsync_QueueNameIsEmpty_ThrowsArgumentNullException()
        {
            // Arrange
            var queueName = string.Empty;

            var client = new SlinqyQueueClient(
                createPhysicalQueueDelegate: name => null
            );

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.CreateAsync(queueName));
        }
    }
}
