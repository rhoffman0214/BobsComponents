namespace BobsComponent.Library.Models.Events;

/// <summary>
/// Event arguments fired when an async operation fails with an error
/// </summary>
public class ComponentErrorEventArgs : AsyncButtonEventArgs
{
    /// <summary>
    /// The error that occurred with rich context
    /// </summary>
    public ComponentError Error { get; init; } = null!;

    /// <summary>
    /// Which attempt failed (1 for first try, >1 if retries occurred)
    /// </summary>
    public int AttemptNumber { get; init; } = 1;

    /// <summary>
    /// Whether the component will automatically retry (consumer can set to true to trigger retry)
    /// </summary>
    public bool WillRetry { get; set; } = false;
}
