using System;

namespace BobsComponent.Library.Models.Events;

/// <summary>
/// Event arguments fired when an async operation completes successfully
/// </summary>
public class AsyncSuccessEventArgs : AsyncButtonEventArgs
{
    /// <summary>
    /// How long the operation took to complete
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Which attempt succeeded (1 for first try, >1 if retries occurred)
    /// </summary>
    public int AttemptNumber { get; init; } = 1;

    /// <summary>
    /// Optional result data from the operation
    /// </summary>
    public object? Result { get; init; }
}
