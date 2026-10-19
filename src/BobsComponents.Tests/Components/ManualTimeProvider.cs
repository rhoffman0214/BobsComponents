namespace BobsComponents.Tests.Components;

/// <summary>A TimeProvider the test moves by hand. Timers fire only when <see cref="Advance"/> passes their due time.</summary>
internal sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _now;
    private readonly List<ManualTimer> _timers = [];

    public ManualTimeProvider(DateTimeOffset start) => _now = start;

    public int ActiveTimers => _timers.Count(t => !t.Disposed);

    public override DateTimeOffset GetUtcNow() => _now;
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new ManualTimer(callback, state);
        _timers.Add(timer);
        return timer;
    }

    public void Advance(TimeSpan by)
    {
        _now += by;
        foreach (var timer in _timers.Where(t => !t.Disposed).ToList()) timer.Fire();
    }

    private sealed class ManualTimer(TimerCallback callback, object? state) : ITimer
    {
        public bool Disposed { get; private set; }
        public void Fire() { if (!Disposed) callback(state); }
        public bool Change(TimeSpan dueTime, TimeSpan p) => true;
        public void Dispose() => Disposed = true;
        public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
    }
}
