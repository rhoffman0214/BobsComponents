namespace BobsComponents.Services;

/// <summary>
/// Where <c>DraftSaver</c> keeps unsaved form state. Payloads are strings (serialize your own form state, JSON for
/// example). The default registration is <see cref="BobMemoryDraftStore"/>, which lasts until the app reloads;
/// register your own implementation (localStorage, IndexedDB, a server) to survive reloads.
/// </summary>
public interface IBobDraftStore
{
    /// <summary>Returns the saved draft for <paramref name="name"/>, or null when there is none.</summary>
    ValueTask<string?> LoadAsync(string name);

    /// <summary>Stores the draft, replacing any earlier one.</summary>
    ValueTask SaveAsync(string name, string content);

    /// <summary>Deletes the draft.</summary>
    ValueTask ClearAsync(string name);
}

/// <summary>In-memory <see cref="IBobDraftStore"/>: survives navigation inside the app, not a page reload.</summary>
public sealed class BobMemoryDraftStore : IBobDraftStore
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _drafts = new();

    public ValueTask<string?> LoadAsync(string name) => new(_drafts.TryGetValue(name, out var v) ? v : null);

    public ValueTask SaveAsync(string name, string content)
    {
        _drafts[name] = content;
        return ValueTask.CompletedTask;
    }

    public ValueTask ClearAsync(string name)
    {
        _drafts.TryRemove(name, out _);
        return ValueTask.CompletedTask;
    }
}
