using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Services.Core.Retries.ErrorDetectionStrategies;
using Microsoft.Services.Core.Retries.RetryStrategies;

namespace Microsoft.Services.Core.Retries
{
    /// <summary>
    /// Provides the base implementation of the retry mechanism for unreliable actions and transient conditions.
    /// </summary>
    public class RetryPolicy
    {
        /// <summary>
        /// Gets the instance of the error detection strategy.
        /// </summary>
        public ITransientErrorDetectionStrategy ErrorDetectionStrategy { get; private set; }

        /// <summary>
        /// Gets the retry strategy.
        /// </summary>
        public RetryStrategy RetryStrategy { get; private set; }

        /// <summary>
        /// An instance of a callback delegate that will be invoked whenever a retry condition is encountered.
        /// </summary>
        public event EventHandler<RetryingEventArgs> Retrying;

        /// <summary>
        /// Notifies the subscribers whenever a retry condition is encountered.
        /// </summary>
        /// <param name="retryCount">The current retry attempt count.</param>
        /// <param name="lastError">The exception that caused the retry conditions to occur.</param>
        /// <param name="delay">The delay that indicates how long the current thread will be suspended before the next iteration is invoked.</param>
        protected virtual void OnRetrying(int retryCount, Exception lastError, TimeSpan delay)
        {
            if (Retrying == null)
                return;
            Retrying(this, new RetryingEventArgs(retryCount, delay, lastError));
        }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with the specified number of retry attempts and parameters 
        /// defining the progressive delay between retries.
        /// </summary>
        /// <param name="errorDetectionStrategy">The ITransientErrorDetectionStrategy that is responsible for detecting transient conditions.</param>
        /// <param name="retryStrategy">The RetryStrategy to use for this retry policy.</param>
        public RetryPolicy(ITransientErrorDetectionStrategy errorDetectionStrategy, RetryStrategy retryStrategy)
        {
            if (errorDetectionStrategy == null)
                throw new ArgumentNullException("errorDetectionStrategy", "The error detection strategy cannot be null.");
            if (retryStrategy == null)
                throw new ArgumentNullException("retryStrategy", "The retry strategy cannot be null.");
            ErrorDetectionStrategy = errorDetectionStrategy;
            RetryStrategy = retryStrategy;
        }

        /// <summary>
        /// Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <param name="action">A delegate that represents the executable action that doesn't return any results.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.ArgumentNullException">action - The action cannot be null.</exception>
        public virtual void ExecuteAction(Action action, CancellationToken cancellationToken)
        {
            if (action == null)
                throw new ArgumentNullException("action", "The action cannot be null.");
            Execute((() =>
            {
                action();
                return (object)null;
            }), cancellationToken);
        }

        /// <summary>
        /// Repetitively executes the specified action while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name="TResult">The type of result expected from the executable action.</typeparam>
        /// <param name="func">A delegate that represents the executable action that returns the result of type <typeparamref name="TResult" />.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// The result from the action.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">func</exception>
        public virtual TResult Execute<TResult>(Func<TResult> func, CancellationToken cancellationToken)
        {
            if (func == null)
                throw new ArgumentNullException("func");
            var retryCount = 1;
            while (true)
            {
                TimeSpan delay;
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    if (!HandleException(retryCount++, ex, out delay))
                    {
                        throw;
                    }
                }
                Task.Delay(delay, cancellationToken).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Repetitively executes the specified asynchronous task while it satisfies the current retry policy.
        /// </summary>
        /// <param name="taskAction">A function that returns a started task (also known as "hot" task).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// A task that will run to completion if the original task completes successfully (either the
        /// first time or after retrying transient failures). If the task fails with a non-transient error or
        /// the retry limit is reached, the returned task will transition to a faulted state and the exception must be observed.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">taskAction</exception>
        public async Task ExecuteAsync(Func<Task> taskAction, CancellationToken cancellationToken)
        {
            if (taskAction == null)
                throw new ArgumentNullException("taskAction");
            var retryCount = 1;
            while (true)
            {
                TimeSpan delay;
                try
                {
                    await taskAction();
                    return;
                }
                catch (Exception ex)
                {
                    if (!HandleException(retryCount++, ex, out delay))
                    {
                        throw;
                    }
                }
                await Task.Delay(delay, cancellationToken);
            }
        }

        /// <summary>
        /// Repeatedly executes the specified asynchronous task while it satisfies the current retry policy.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="taskFunc">A function that returns a started task (also known as "hot" task).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// Returns a task that will run to completion if the original task completes successfully (either the
        /// first time or after retrying transient failures). If the task fails with a non-transient error or
        /// the retry limit is reached, the returned task will transition to a faulted state and the exception must be observed.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">taskFunc</exception>
        public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> taskFunc, CancellationToken cancellationToken)
        {
            if (taskFunc == null)
                throw new ArgumentNullException("taskFunc");
            var retryCount = 1;

            while (true)
            {
                TimeSpan delay;
                try
                {
                    return await taskFunc();
                }
                catch (Exception ex)
                {
                    if (!HandleException(retryCount++, ex, out delay))
                    {
                        throw;
                    }
                }
                await Task.Delay(delay, cancellationToken);
            }
        }

        private bool HandleException(int retryCount, Exception lastException, out TimeSpan delay)
        {
            var shouldRetry = RetryStrategy.GetShouldRetry();
            if (ErrorDetectionStrategy.IsTransient(lastException) && shouldRetry(retryCount, lastException, out delay))
            {
                OnRetrying(retryCount, lastException, delay);
                if (delay.TotalMilliseconds < 0.0)
                    delay = TimeSpan.Zero;
                return true;
            }
            delay = TimeSpan.Zero;
            return false;
        }
    }
}
