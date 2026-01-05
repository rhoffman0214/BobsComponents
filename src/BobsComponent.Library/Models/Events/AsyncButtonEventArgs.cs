namespace BobsComponent.Library.Models.Events;

/// <summary>
/// Base event arguments for all AsyncButton async operations
/// </summary>
public class AsyncButtonEventArgs
{
    /// <summary>
    /// Unique identifier for this operation
    /// </summary>
    public string OperationId { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the operation can be cancelled (set to false to prevent execution)
    /// </summary>
    public bool CanCancel { get; set; } = true;

    /// <summary>
    /// Cancellation token for the operation
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}
