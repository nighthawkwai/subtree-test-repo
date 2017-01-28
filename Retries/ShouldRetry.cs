using System;

namespace Microsoft.Services.Core.Retries
{
    /// <summary>
    /// Defines a callback delegate that will be invoked whenever a retry condition is encountered.
    /// </summary>
    /// <param name="retryCount">The current retry attempt count.</param>
    /// <param name="lastException">The exception that caused the retry conditions to occur.</param>
    /// <param name="delay">The delay that indicates how long the current thread will be suspended before the next iteration is invoked.</param>
    /// <returns>
    /// Whether the retry should happen.
    /// </returns>
    public delegate bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay);
}