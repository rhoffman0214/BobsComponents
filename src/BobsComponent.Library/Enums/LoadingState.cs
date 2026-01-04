namespace BobsComponent.Library.Enums;

/// <summary>
/// Represents the state of an asynchronous operation
/// </summary>
public enum LoadingState
{
    /// <summary>
    /// Operation has not started or has been reset
    /// </summary>
    Idle,

    /// <summary>
    /// Operation is currently in progress
    /// </summary>
    Loading,

    /// <summary>
    /// Operation completed successfully
    /// </summary>
    Success,

    /// <summary>
    /// Operation failed with an error
    /// </summary>
    Error
}
