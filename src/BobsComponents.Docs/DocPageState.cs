using BobsComponents.Docs.Generation;

namespace BobsComponents.Docs;

/// <summary>
/// Shared between a component page and its demo frames: the latest rendered markup of each frame (reported by the
/// browser) and the DOM rows built from it, so the DOM map and the "sits in" column read the live demo.
/// </summary>
public sealed class DocPageState
{
    private readonly Dictionary<string, IReadOnlyList<DomRow>> _frames = new(StringComparer.Ordinal);

    public event Action? Changed;

    public IReadOnlyDictionary<string, IReadOnlyList<DomRow>> Frames => _frames;

    public IEnumerable<DomRow> AllRows => _frames.Values.SelectMany(r => r);

    public void SetFrameHtml(string key, string html)
    {
        _frames[key] = DomMap.Build(html);
        Changed?.Invoke();
    }
}
