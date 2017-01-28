using System;

namespace Microsoft.Services.Core.Retries.RetryStrategies
{
    /// <summary>
    /// A retry strategy with backoff parameters for calculating the exponential delay between retries.
    /// </summary>
    public class ExponentialBackoff : RetryStrategy
    {
        private readonly TimeSpan _minBackoff;
        private readonly TimeSpan _maxBackoff;
        private readonly TimeSpan _deltaBackoff;
        private static readonly Random Random = new Random();

        /// <summary>
        /// Initializes a new instance of the ExponentialBackoff class with the specified name, retry settings, and fast retry option.
        /// </summary>
        /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
        /// <param name="minBackoff">The minimum backoff time</param><param name="maxBackoff">The maximum backoff time.</param>
        /// <param name="deltaBackoff">The value that will be used to calculate a random delta in the exponential delay between retries.</param>
        public ExponentialBackoff(int maxRetryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
        {
            MaxRetryCount = maxRetryCount;
            _minBackoff = minBackoff;
            _maxBackoff = maxBackoff;
            _deltaBackoff = deltaBackoff;
        }

        /// <summary>
        /// Returns the corresponding ShouldRetry delegate.
        /// </summary>
        /// <returns>
        /// The ShouldRetry delegate.
        /// </returns>
        public override ShouldRetry GetShouldRetry()
        {
            return ((int currentRetryCount, Exception lastException, out TimeSpan retryInterval) =>
            {
                if (currentRetryCount < MaxRetryCount)
                {
                    var delayInMilliseconds =
                        (int)
                            Math.Min(
                                _minBackoff.TotalMilliseconds +
                                (int)
                                    ((Math.Pow(2.0, currentRetryCount) - 1.0) *
                                     Random.Next((int)(_deltaBackoff.TotalMilliseconds * 0.8),
                                         (int)(_deltaBackoff.TotalMilliseconds * 1.2))), _maxBackoff.TotalMilliseconds);
                    retryInterval = TimeSpan.FromMilliseconds(delayInMilliseconds);
                    return true;
                }
                retryInterval = TimeSpan.Zero;
                return false;
            });
        }
    }
}