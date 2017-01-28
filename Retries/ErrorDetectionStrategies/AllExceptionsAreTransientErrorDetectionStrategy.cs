using System;

namespace Microsoft.Services.Core.Retries.ErrorDetectionStrategies
{
    /// <summary>
    /// Always returns true for every Exception.
    /// </summary>
    [Serializable]
    public sealed class AllExceptionsAreTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        public bool IsTransient(Exception ex)
        {
            return true;
        }

        /// <summary>
        /// Gets the extended details for the exception provided.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>The extended details for the exception provided.</returns>
        public string GetExtendedDetails(Exception ex)
        {
            return ExceptionHelper.GetIndentedExceptionString(ex);
        }
    }
}