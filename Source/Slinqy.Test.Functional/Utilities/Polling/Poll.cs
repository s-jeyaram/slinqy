namespace Slinqy.Test.Functional.Utilities.Polling
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Provides common polling functionality.
    /// </summary>
    internal static class Poll
    {
        /// <summary>
        /// Polls the specified function until a certain criteria is met.
        /// </summary>
        /// <typeparam name="T">Specifies the return type of the from function.</typeparam>
        /// <param name="from">Specifies the function to use to retrieve the value.</param>
        /// <param name="until">Specifies the function to use to test the value.</param>
        /// <param name="interval">Specifies how often to get the latest value via the from function.</param>
        /// <param name="maxDuration">Specifies the max amount of time to wait for the right value before giving up.</param>
        public
        static
        void
        Value<T>(
            Func<T>       from,
            Func<T, bool> until,
            TimeSpan      interval,
            TimeSpan      maxDuration)
        {
            var stopwatch = Stopwatch.StartNew();

            while (until(from()) == false)
            {
                if (stopwatch.Elapsed > maxDuration)
                    throw new PollTimeoutException(maxDuration);

                Thread.Sleep(interval);
            }
        }
    }
}
