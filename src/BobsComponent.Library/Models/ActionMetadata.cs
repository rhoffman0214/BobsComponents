using System;
using BobsComponent.Library.Enums;

namespace BobsComponent.Library.Models;

/// <summary>
/// Tracks metadata for an individual asynchronous action
/// </summary>
public class ActionMetadata
{
    /// <summary>
    /// Unique identifier for the action
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name for the action
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Current state of the action
    /// </summary>
    public LoadingState State { get; set; } = LoadingState.Idle;

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int Progress { get; set; } = 0;

    /// <summary>
    /// Timestamp when the action started
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Timestamp when the action completed (success or error)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Duration of the action if completed
    /// </summary>
    public TimeSpan? Duration => EndTime.HasValue && StartTime.HasValue
        ? EndTime.Value - StartTime.Value
        : null;

    /// <summary>
    /// Error message if the action failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
