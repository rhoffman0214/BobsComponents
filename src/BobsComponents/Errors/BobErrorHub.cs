using BobsComponents.Options;

namespace BobsComponents.Errors;

/// <summary>
/// Collects every classified error in one place: boundaries publish here, components send failures here
/// from fire-and-forget work, and the dev kit Errors panel reads from it.
/// </summary>
public interface IBobErrorHub
{
    IReadOnlyList<BobError> Errors { get; }
    event Action<BobError>? ErrorReported;
    /// <summary>Raised after any change to <see cref="Errors"/>, including Clear.</summary>
    event Action? Changed;
    /// <summary>Classify and record an exception. Returns the classified error.</summary>
    BobError Report(Exception exception, string? component = null);
    /// <summary>Record an error that is already classified.</summary>
    void Report(BobError error);
    void Clear();
}

public sealed class BobErrorHub : IBobErrorHub
{
    private readonly IBobErrorClassifier _classifier;
    private readonly BobOptions _options;
    private readonly List<BobError> _errors = [];
    private readonly object _gate = new();

    public BobErrorHub(IBobErrorClassifier classifier, BobOptions options)
    {
        _classifier = classifier;
        _options = options;
    }

    public event Action<BobError>? ErrorReported;
    public event Action? Changed;

    public IReadOnlyList<BobError> Errors
    {
        get { lock (_gate) return _errors.ToArray(); }
    }

    public BobError Report(Exception exception, string? component = null)
    {
        var error = _classifier.Classify(exception, component);
        Report(error);
        return error;
    }

    public void Report(BobError error)
    {
        lock (_gate)
        {
            _errors.Add(error);
            while (_errors.Count > Math.Max(1, _options.DevKit.MaxEntries)) _errors.RemoveAt(0);
        }
        ErrorReported?.Invoke(error);
        Changed?.Invoke();
    }

    public void Clear()
    {
        lock (_gate) _errors.Clear();
        Changed?.Invoke();
    }
}
