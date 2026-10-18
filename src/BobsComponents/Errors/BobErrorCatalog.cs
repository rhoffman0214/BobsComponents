using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BobsComponents.Errors;

/// <summary>
/// One documented error. The catalog file (error-catalog.json) is the single source for the docs
/// error pages and the "Learn more" link on the runtime card.
/// </summary>
public sealed record BobErrorCatalogEntry
{
    public required string Code { get; init; }
    public required BobErrorOrigin Origin { get; init; }
    /// <summary>Short name, e.g. "Required parameter missing".</summary>
    public required string Title { get; init; }
    /// <summary>The message shape, with {placeholders} for the facts. The runtime card always shows the real message, not this.</summary>
    public required string Message { get; init; }
    public required string Cause { get; init; }
    public required string Fix { get; init; }
    /// <summary>Component names that can raise it; empty means any.</summary>
    public IReadOnlyList<string> Components { get; init; } = [];
}

/// <summary>
/// Error catalog format: a JSON array of <see cref="BobErrorCatalogEntry"/> with origin written as the enum
/// name. The library ships its own file as an embedded resource; apps and other packages add theirs with
/// <see cref="Add"/> using the same format and their own code prefix.
/// </summary>
public sealed class BobErrorCatalog
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly Dictionary<string, BobErrorCatalogEntry> _entries = new(StringComparer.Ordinal);

    public IEnumerable<BobErrorCatalogEntry> All => _entries.Values.OrderBy(e => e.Code, StringComparer.Ordinal);

    public BobErrorCatalogEntry? Get(string code) => _entries.GetValueOrDefault(code);

    public void Add(IEnumerable<BobErrorCatalogEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (!_entries.TryAdd(entry.Code, entry))
                throw new InvalidOperationException($"Error catalog already has an entry for code '{entry.Code}'.");
        }
    }

    public void AddJson(string json) => Add(Parse(json));

    public static IReadOnlyList<BobErrorCatalogEntry> Parse(string json) =>
        JsonSerializer.Deserialize<List<BobErrorCatalogEntry>>(json, JsonOptions)
        ?? throw new JsonException("Error catalog JSON was empty.");

    /// <summary>The catalog shipped inside this package.</summary>
    public static BobErrorCatalog CreateDefault()
    {
        var catalog = new BobErrorCatalog();
        using var stream = typeof(BobErrorCatalog).Assembly.GetManifestResourceStream("BobsComponents.error-catalog.json")
            ?? throw new InvalidOperationException("Embedded error-catalog.json is missing from BobsComponents.");
        using var reader = new StreamReader(stream);
        catalog.AddJson(reader.ReadToEnd());
        return catalog;
    }
}
