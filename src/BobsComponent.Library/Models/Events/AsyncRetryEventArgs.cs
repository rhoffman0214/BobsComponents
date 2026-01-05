using System;

namespace BobsComponent.Library.Models.Events;

/// <summary>
/// Event arguments fired when an async operation is about to be retried after a failure
/// </summary>
public class AsyncRetryEventArgs : AsyncButtonEventArgs
{
    /// <summary>
    /// Which retry attempt this is (1 for first retry, 2 for second, etc.)
    /// </summary>
    public int AttemptNumber { get; init; } = 1;

    /// <summary>
    /// The error that caused the retry
    /// </summary>
    public ComponentError? LastError { get; init; }

    /// <summary>
    /// How long the component will wait before retrying (exponential backoff)
    /// </summary>
    public TimeSpan BackoffDelay { get; init; }
}
