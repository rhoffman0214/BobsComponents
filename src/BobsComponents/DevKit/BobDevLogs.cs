using BobsComponents.Options;

namespace BobsComponents.DevKit;

/// <summary>Thread-safe bounded log shared by the dev kit panels.</summary>
public abstract class BobBoundedLog<T>
{
    private readonly BobOptions _options;
    private readonly List<T> _items = [];
    private readonly object _gate = new();

    protected BobBoundedLog(BobOptions options) => _options = options;

    public event Action? Changed;

    public IReadOnlyList<T> Items
    {
        get { lock (_gate) return _items.ToArray(); }
    }

    protected void AddItem(T item)
    {
        lock (_gate)
        {
            _items.Add(item);
            while (_items.Count > Math.Max(1, _options.DevKit.MaxEntries)) _items.RemoveAt(0);
        }
        Changed?.Invoke();
    }

    public void Clear()
    {
        lock (_gate) _items.Clear();
        Changed?.Invoke();
    }
}

/// <summary>One HTTP request seen by <see cref="BobNetworkHandler"/>.</summary>
public sealed record BobNetworkEntry(
    DateTimeOffset Time,
    string Method,
    string Url,
    int? Status,
    string? Reason,
    TimeSpan Duration,
    string? RequestBody,
    string? ResponseBody,
    string? TransportError);

public sealed class BobNetworkLog : BobBoundedLog<BobNetworkEntry>
{
    public BobNetworkLog(BobOptions options) : base(options) { }
    public void Add(BobNetworkEntry entry) => AddItem(entry);
}

/// <summary>One JavaScript interop call seen by <see cref="BobLoggingJSRuntime"/>.</summary>
public sealed record BobInteropEntry(
    DateTimeOffset Time,
    string Target,
    string Identifier,
    string Arguments,
    TimeSpan Duration,
    string? Failure);

public sealed class BobInteropLog : BobBoundedLog<BobInteropEntry>
{
    public BobInteropLog(BobOptions options) : base(options) { }
    public void Add(BobInteropEntry entry) => AddItem(entry);
}
