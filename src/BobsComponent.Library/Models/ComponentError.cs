namespace BobsComponent.Library.Models;

/// <summary>
/// Represents a component error with rich context for debugging and user feedback
/// </summary>
public class ComponentError
{
    /// <summary>
    /// Unique identifier for this error instance (for correlation)
    /// </summary>
    public string ErrorId { get; init; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// Unique identifier for the operation that failed
    /// </summary>
    public string? OperationId { get; init; }

    /// <summary>
    /// The underlying exception (sanitized)
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// User-friendly error message (safe to display)
    /// </summary>
    public string Message { get; init; } = "An error occurred";

    /// <summary>
    /// Developer-friendly error message with context (for logging/debugging)
    /// </summary>
    public string? DeveloperMessage { get; init; }

    /// <summary>
    /// Error code for categorization
    /// </summary>
    public string ErrorCode { get; init; } = "UNKNOWN_ERROR";

    /// <summary>
    /// When the error occurred
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Source component type
    /// </summary>
    public string Source { get; init; } = "Unknown";

    /// <summary>
    /// Additional context (component state, user data, etc.)
    /// </summary>
    public Dictionary<string, object> Context { get; init; } = new();

    /// <summary>
    /// Whether this error is recoverable (can retry)
    /// </summary>
    public bool IsRecoverable { get; init; } = true;

    /// <summary>
    /// Suggested action for user
    /// </summary>
    public string? SuggestedAction { get; init; }

    /// <summary>
    /// Creates a ComponentError from an exception
    /// </summary>
    /// <param name="ex">The exception that occurred</param>
    /// <param name="source">The component where the error occurred</param>
    /// <param name="operationId">Optional operation identifier for correlation</param>
    /// <param name="context">Optional additional context</param>
    /// <returns>A ComponentError with rich error information</returns>
    public static ComponentError FromException(
        Exception ex,
        string source,
        string? operationId = null,
        Dictionary<string, object>? context = null)
    {
        var errorCode = DetermineErrorCode(ex);
        var isRecoverable = DetermineRecoverability(ex);
        var userMessage = GenerateUserMessage(ex, errorCode);

        return new ComponentError
        {
            Exception = ex,
            Message = userMessage,
            DeveloperMessage = $"{ex.GetType().Name}: {ex.Message}",
            ErrorCode = errorCode,
            Source = source,
            OperationId = operationId,
            Context = context ?? new(),
            IsRecoverable = isRecoverable,
            SuggestedAction = GenerateSuggestedAction(errorCode, isRecoverable)
        };
    }

    /// <summary>
    /// Determines the error code based on exception type
    /// </summary>
    private static string DetermineErrorCode(Exception ex)
    {
        return ex switch
        {
            OperationCanceledException => "OPERATION_CANCELLED",
            TimeoutException => "OPERATION_TIMEOUT",
            HttpRequestException => "NETWORK_ERROR",
            ArgumentException => "INVALID_ARGUMENT",
            ArgumentNullException => "INVALID_ARGUMENT",
            InvalidOperationException => "INVALID_STATE",
            UnauthorizedAccessException => "UNAUTHORIZED",
            NotImplementedException => "NOT_IMPLEMENTED",
            NotSupportedException => "NOT_SUPPORTED",
            _ => "UNKNOWN_ERROR"
        };
    }

    /// <summary>
    /// Determines if the error is recoverable based on exception type
    /// </summary>
    private static bool DetermineRecoverability(Exception ex)
    {
        return ex switch
        {
            // Non-recoverable errors
            ArgumentException => false,
            ArgumentNullException => false,
            InvalidOperationException => false,
            NotImplementedException => false,
            NotSupportedException => false,

            // Recoverable errors (network, timeout, cancellation)
            HttpRequestException => true,
            TimeoutException => true,
            OperationCanceledException => false, // Don't retry cancellations

            // Unknown errors are considered recoverable (conservative approach)
            _ => true
        };
    }

    /// <summary>
    /// Generates a user-friendly error message based on error code
    /// </summary>
    private static string GenerateUserMessage(Exception ex, string errorCode)
    {
        return errorCode switch
        {
            "OPERATION_CANCELLED" => "Operation was cancelled",
            "OPERATION_TIMEOUT" => "Operation timed out. Please try again.",
            "NETWORK_ERROR" => "Network error. Please check your connection.",
            "INVALID_ARGUMENT" => "Invalid input provided",
            "INVALID_STATE" => "Operation cannot be performed in current state",
            "UNAUTHORIZED" => "You are not authorized to perform this operation",
            "NOT_IMPLEMENTED" => "This feature is not yet implemented",
            "NOT_SUPPORTED" => "This operation is not supported",
            _ => "An unexpected error occurred. Please try again."
        };
    }

    /// <summary>
    /// Generates a suggested action for the user based on error code
    /// </summary>
    private static string GenerateSuggestedAction(string errorCode, bool isRecoverable)
    {
        if (!isRecoverable)
            return "Please refresh the page and try again, or contact support if the problem persists.";

        return errorCode switch
        {
            "NETWORK_ERROR" => "Check your internet connection and retry.",
            "OPERATION_TIMEOUT" => "The operation is taking longer than expected. Retry or wait a moment.",
            "UNAUTHORIZED" => "Please log in and try again.",
            _ => "Click retry to attempt the operation again."
        };
    }
}
