namespace Slinqy.Test.Functional.Utilities.Strings
{
    using System;
    using System.Linq;

    /// <summary>
    /// Provides extensions on the String class.
    /// </summary>
    public static class StringUtilities
    {
        /// <summary>
        /// Generates a random string of the specified length.
        /// </summary>
        /// <param name="length">Specifies the number of characters to include in the string.</param>
        /// <returns>Returns the randomly generated string.</returns>
        public
        static
        string
        RandomString(
            int length)
        {
            const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            var random = new Random();

            return new string(Enumerable.Repeat(Chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
