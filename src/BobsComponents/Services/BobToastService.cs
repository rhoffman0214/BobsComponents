namespace BobsComponents.Services;

public enum BobToastLevel { Info, Success, Error }

/// <summary>A message on screen. <see cref="Duration"/> null means "use the default from BobToastOptions".</summary>
public sealed record BobToast(Guid Id, string Text, BobToastLevel Level, TimeSpan? Duration = null);

/// <summary>
/// Lightweight pub/sub so any component can show a confirmation toast without prop-drilling through
/// layouts. <c>BobToastHost</c> (mounted once in the layout) is the only subscriber that renders anything.
/// </summary>
public interface IBobToastService
{
    event Action<BobToast>? OnShow;
    void Show(string text, BobToastLevel level = BobToastLevel.Success, TimeSpan? duration = null);
}

public sealed class BobToastService : IBobToastService
{
    public event Action<BobToast>? OnShow;

    public void Show(string text, BobToastLevel level = BobToastLevel.Success, TimeSpan? duration = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        OnShow?.Invoke(new BobToast(Guid.NewGuid(), text, level, duration));
    }
}
