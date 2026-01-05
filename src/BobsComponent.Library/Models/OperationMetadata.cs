using System;
using BobsComponent.Library.Enums;

namespace BobsComponent.Library.Models;

/// <summary>
/// Tracks metadata about a single operation execution
/// </summary>
public class OperationMetadata
{
    /// <summary>
    /// Unique identifier for this operation
    /// </summary>
    public string OperationId { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// When the operation started
    /// </summary>
    public DateTime StartTime { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When the operation ended (success or failure)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Duration of the operation
    /// </summary>
    public TimeSpan Duration => EndTime.HasValue
        ? EndTime.Value - StartTime
        : TimeSpan.Zero;

    /// <summary>
    /// Number of attempts made (including retries)
    /// </summary>
    public int AttemptCount { get; set; } = 1;

    /// <summary>
    /// Whether the operation ultimately succeeded
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Final state of the operation
    /// </summary>
    public LoadingState FinalState { get; set; }

    /// <summary>
    /// Error that caused the operation to fail (if any)
    /// </summary>
    public ComponentError? Error { get; set; }

    /// <summary>
    /// Operation result data (if any)
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Gets a summary string for logging
    /// </summary>
    public string GetSummary()
    {
        var status = Success ? "SUCCESS" : "FAILED";
        var durationMs = Duration.TotalMilliseconds;
        var attempts = AttemptCount > 1 ? $" after {AttemptCount} attempts" : "";

        if (Error != null)
        {
            return $"[{OperationId}] {status} in {durationMs:F0}ms{attempts} - {Error.ErrorCode}: {Error.Message}";
        }

        return $"[{OperationId}] {status} in {durationMs:F0}ms{attempts}";
    }
}
