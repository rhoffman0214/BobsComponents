using System;
using System.Net.Http;

namespace BobsComponent.Library.Models;

/// <summary>
/// Configuration for automatic retry behavior with exponential backoff
/// </summary>
public class RetryConfiguration
{
    /// <summary>
    /// Whether retry is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts (including the initial attempt)
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// Initial delay before first retry
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Multiplier for exponential backoff (e.g., 2.0 doubles the delay each time)
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Maximum delay between retries (caps exponential growth)
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Optional custom logic to determine if a specific exception should be retried
    /// Return true to retry, false to fail immediately
    /// </summary>
    public Func<Exception, bool>? ShouldRetry { get; set; }

    /// <summary>
    /// Creates a default retry configuration optimized for network operations
    /// </summary>
    public static RetryConfiguration ForNetworkOperations()
    {
        return new RetryConfiguration
        {
            Enabled = true,
            MaxAttempts = 5,
            InitialDelay = TimeSpan.FromMilliseconds(500),
            BackoffMultiplier = 2.0,
            MaxDelay = TimeSpan.FromSeconds(16),
            ShouldRetry = ex => ex is HttpRequestException or TimeoutException
        };
    }

    /// <summary>
    /// Creates a default retry configuration optimized for fast operations
    /// </summary>
    public static RetryConfiguration ForFastOperations()
    {
        return new RetryConfiguration
        {
            Enabled = true,
            MaxAttempts = 3,
            InitialDelay = TimeSpan.FromMilliseconds(100),
            BackoffMultiplier = 1.5,
            MaxDelay = TimeSpan.FromSeconds(1)
        };
    }

    /// <summary>
    /// Creates a retry configuration with no retries (fail fast)
    /// </summary>
    public static RetryConfiguration NoRetry()
    {
        return new RetryConfiguration
        {
            Enabled = false,
            MaxAttempts = 1
        };
    }
}
