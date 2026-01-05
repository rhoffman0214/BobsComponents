using System;
using System.Collections.Generic;
using System.Linq;

namespace BobsComponent.Library.Models;

/// <summary>
/// Tracks error statistics and patterns for a component
/// </summary>
public class ErrorMetadata
{
    /// <summary>
    /// Total number of errors that have occurred
    /// </summary>
    public int TotalErrors { get; set; }

    /// <summary>
    /// Number of consecutive errors (reset on success)
    /// </summary>
    public int ConsecutiveErrors { get; set; }

    /// <summary>
    /// When the most recent error occurred
    /// </summary>
    public DateTime? LastErrorTime { get; set; }

    /// <summary>
    /// When the first error occurred in the current session
    /// </summary>
    public DateTime? FirstErrorTime { get; set; }

    /// <summary>
    /// Count of errors by error code
    /// </summary>
    public Dictionary<string, int> ErrorCodeCounts { get; set; } = new();

    /// <summary>
    /// Average time between errors
    /// </summary>
    public TimeSpan? AverageTimeBetweenErrors { get; set; }

    /// <summary>
    /// Records a new error occurrence
    /// </summary>
    /// <param name="error">The error that occurred</param>
    public void RecordError(ComponentError error)
    {
        TotalErrors++;
        ConsecutiveErrors++;

        var now = error.Timestamp;
        LastErrorTime = now;
        FirstErrorTime ??= now;

        // Track error code frequency
        if (!ErrorCodeCounts.ContainsKey(error.ErrorCode))
            ErrorCodeCounts[error.ErrorCode] = 0;
        ErrorCodeCounts[error.ErrorCode]++;

        // Calculate average time between errors
        if (FirstErrorTime.HasValue && TotalErrors > 1)
        {
            var totalTime = now - FirstErrorTime.Value;
            AverageTimeBetweenErrors = TimeSpan.FromMilliseconds(
                totalTime.TotalMilliseconds / (TotalErrors - 1));
        }
    }

    /// <summary>
    /// Records a successful operation (resets consecutive error count)
    /// </summary>
    public void RecordSuccess()
    {
        ConsecutiveErrors = 0;
    }

    /// <summary>
    /// Gets the error rate (errors per minute) over the session
    /// </summary>
    public double GetErrorRate()
    {
        if (!FirstErrorTime.HasValue || !LastErrorTime.HasValue || TotalErrors == 0)
            return 0;

        var duration = LastErrorTime.Value - FirstErrorTime.Value;
        if (duration.TotalMinutes < 0.001) // Less than a millisecond
            return TotalErrors; // Treat as instantaneous

        return TotalErrors / duration.TotalMinutes;
    }

    /// <summary>
    /// Gets the most common error code
    /// </summary>
    public string? GetMostCommonErrorCode()
    {
        if (ErrorCodeCounts.Count == 0)
            return null;

        return ErrorCodeCounts
            .OrderByDescending(kvp => kvp.Value)
            .First()
            .Key;
    }
}
