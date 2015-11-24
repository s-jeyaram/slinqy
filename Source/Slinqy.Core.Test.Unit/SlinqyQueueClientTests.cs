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
        private readonly SlinqyQueueClient client;
        private readonly IPhysicalQueueService fakePhysicalQueueService = A.Fake<IPhysicalQueueService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SlinqyQueueClientTests"/> class.
        /// </summary>
        public
        SlinqyQueueClientTests()
        {
            this.client = new SlinqyQueueClient(this.fakePhysicalQueueService);
        }

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
            const string queueName = "test";

            // Act
            await this.client.CreateAsync(queueName);

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

            // Act
            Func<Task<SlinqyQueue>> action = async () => await this.client.CreateAsync(queueName);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        /// <summary>
        /// Verifies that a SlinqyQueue instance is added and returned when one doesn't already exist.
        /// </summary>
        [Fact]
        public
        void
        Get_SlinqyQueueDoesNotExistInCollection_IsReturned()
        {
            // Arrange
            const string queueName = "test";

            // Act
            var actualQueueName = this.client.Get(queueName).Name;

            // Assert
            Assert.Equal(queueName, actualQueueName);
        }
    }
}