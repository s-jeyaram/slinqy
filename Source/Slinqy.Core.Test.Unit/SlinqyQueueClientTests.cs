namespace Slinqy.Core.Test.Unit
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Xunit;

    /// <summary>
    /// Tests functions of the SlinqyQueueClient class.
    /// </summary>
    public class SlinqyQueueClientTests
    {
        private readonly IPhysicalQueueService fakePhysicalQueueService = A.Fake<IPhysicalQueueService>();

        /// <summary>
        /// Verifies the constructor checks for null values.
        /// </summary>
        [Fact]
        public
        static
        void
        Constructor_PhysicalQueueServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SlinqyQueueClient(null));
        }

        /// <summary>
        /// Verifies that the create delegate is called when CreateAsync is called.
        /// </summary>
        /// <returns>Returns the async Task.</returns>
        [Fact]
        public
        async Task
        CreateAsync_QueueNameValid_CreateDelegateInvoked()
        {
            // Arrange
            var client = new SlinqyQueueClient(this.fakePhysicalQueueService);

            // Act
            await client.CreateAsync("test");

            // Assert
            A.CallTo(() =>
                this.fakePhysicalQueueService.CreateQueue(A<string>.Ignored)
            ).MustHaveHappened();
        }

        /// <summary>
        /// Verifies that the queue name is valid.
        /// </summary>
        /// <returns>Returns the async Task.</returns>
        [Fact]
        public
        async Task
        CreateAsync_QueueNameIsEmpty_ThrowsArgumentNullException()
        {
            // Arrange
            string queueName = null;

            var client = new SlinqyQueueClient(
                this.fakePhysicalQueueService
            );

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.CreateAsync(queueName));
        }
    }
}
