namespace Slinqy.Test.Functional.Utilities.Polling
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <summary>
    /// Thrown when the Poll class cannot find the desired value.
    /// </summary>
    [Serializable]
    public class PollTimeoutException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PollTimeoutException"/> class.
        /// </summary>
        /// <param name="maxPollDuration">Specifies what the max poll duration was.</param>
        public
        PollTimeoutException(
            TimeSpan maxPollDuration)
                : base(string.Format(CultureInfo.InvariantCulture, "Could not find the desired value after {0}.", maxPollDuration))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PollTimeoutException"/> class.
        /// </summary>
        public
        PollTimeoutException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PollTimeoutException"/> class.
        /// </summary>
        /// <param name="message">Specifies the exception message.</param>
        public
        PollTimeoutException(
            string message)
                : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PollTimeoutException"/> class.
        /// </summary>
        /// <param name="message">Specifies the exception message.</param>
        /// <param name="innerException">Specifies an inner exception.</param>
        public
        PollTimeoutException(
            string      message,
            Exception   innerException)
                : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PollTimeoutException"/> class.
        /// </summary>
        /// <param name="info">Specifies serialization information.</param>
        /// <param name="context">Specifies an existing serialization context.</param>
        protected
        PollTimeoutException(
            SerializationInfo   info,
            StreamingContext    context)
                : base(info, context)
        {
        }
    }
}
