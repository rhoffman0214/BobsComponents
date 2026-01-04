namespace BobsComponent.Library.Models;

/// <summary>
/// Represents the result of an asynchronous operation
/// </summary>
public class AsyncOperationResult
{
    /// <summary>
    /// Indicates whether the operation completed successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Optional data payload returned by the operation
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Creates a successful operation result
    /// </summary>
    public static AsyncOperationResult CreateSuccess(object? data = null)
    {
        return new AsyncOperationResult
        {
            Success = true,
            Data = data
        };
    }

    /// <summary>
    /// Creates a failed operation result with an error message
    /// </summary>
    public static AsyncOperationResult CreateError(string errorMessage)
    {
        return new AsyncOperationResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
