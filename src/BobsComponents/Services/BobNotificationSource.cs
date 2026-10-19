namespace BobsComponents.Services;

/// <summary>One entry in the <c>NotificationBell</c> panel.</summary>
/// <param name="Message">The text to show.</param>
/// <param name="At">When it happened.</param>
/// <param name="LinkUrl">Optional place to go when the entry is activated; the bell raises <c>OnNavigate</c> with it.</param>
public sealed record BobNotification(string Message, DateTimeOffset At, string? LinkUrl = null);

/// <summary>
/// Where <c>NotificationBell</c> gets its data. Implement it over your own API, then pass it as the bell's
/// <c>Source</c> parameter or register it as a service.
/// </summary>
public interface IBobNotificationSource
{
    /// <summary>The number of notifications the user has not seen yet.</summary>
    Task<int> GetUnreadCountAsync();

    /// <summary>The notifications to list, newest first.</summary>
    Task<IReadOnlyList<BobNotification>> ListAsync();

    /// <summary>Marks everything read. Called after the panel opens and the list has loaded.</summary>
    Task MarkAllReadAsync();
}
