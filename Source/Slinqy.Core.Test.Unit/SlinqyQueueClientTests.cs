namespace Slinqy.Core.Test.Unit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Xunit;

    /// <summary>
    /// Tests functions of the SlinqyQueueClient class.
    /// </summary>
    public static class SlinqyQueueClientTests
    {
        /// <summary>
        /// Verifies the constructor checks for null values.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Underscores are part of the test naming convention.")]
        [Fact]
        public 
        static 
        void 
        Constructor_CreatePhysicalQueueDelegateIsNull_ThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SlinqyQueueClient(null));
        }
    }
}
