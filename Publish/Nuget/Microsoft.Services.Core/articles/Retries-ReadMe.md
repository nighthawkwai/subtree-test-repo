# Retries

A retry policy is made up of two parts:
* Error detection strategy - defines which errors are transient
* Retry strategy - defines what to do once a transient error is hit.

# Getting Started

## Quick Start

Create a new RetryPolicy and provide an error detection strategy (`ITransientErrorDetectionStrategy`) and a  retry strategy (`RetryStrategy`).

```csharp
var retryStrategy = new ExponentialBackoff(RetryStrategy.DefaultClientRetryCount, RetryStrategy.DefaultMinBackoff, RetryStrategy.DefaultMaxBackoff, RetryStrategy.DefaultRetryInterval);
var retryPolicy = new RetryPolicy(new AllExceptionsAreTransientErrorDetectionStrategy(), retryStrategy);
retryPolicy.ExecuteAction(DoSomething);
```

### Retry Policy Methods
All methods in the `RetryPolicy` class take in a CancellationToken to allow for a return even while the wait (Task.Delay) is happening.
#### ExecuteAction
ExecuteAction is a synchronous call that takes in an Action to execute. This executes the action given the strategies. An Action cannot have a return type, so this method does not return anything.
#### Execute
Execute is a synchronous call that takes in a Func<T> to execute. This executes the func given the strategies and returns T.
#### ExecuteAsync
There are two overloads of ExecuteAsync, both of which are asynchronous. One takes in a Func<Task> and returns a Task, and the other takes in a Func<Task<T>> and returns a Task<T> depending on the overload.

### Custom Retry Policy
In order to create a retry policy that fits your needs, all you need is a fitting error detection strategy and retry strategy. To create a custom error detection strategy, create a new implementation of ITransientErrorDetectionStrategy. To create a custom retry strategy, create a new implementation of RetryStrategy.

Example RetryStrategy:
```csharp
public class ExponentialBackoff : RetryStrategy
{
    private readonly TimeSpan _minBackoff;
    private readonly TimeSpan _maxBackoff;
    private readonly TimeSpan _deltaBackoff;
    private static readonly Random Random = new Random();

    public ExponentialBackoff(int maxRetryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
    {
        MaxRetryCount = maxRetryCount;
        _minBackoff = minBackoff;
        _maxBackoff = maxBackoff;
        _deltaBackoff = deltaBackoff;
    }

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
```

Example ITransientErrorDetectionStrategy:
```csharp
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
```