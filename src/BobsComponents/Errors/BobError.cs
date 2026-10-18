namespace BobsComponents.Errors;

/// <summary>One link of an exception chain, kept as plain text so a <see cref="BobError"/> can be logged and shown.</summary>
public sealed record BobErrorChainItem(string Type, string Message, string? StackTrace);

/// <summary>
/// The single shape every error takes after classification. <see cref="Message"/> is always the exact
/// message from the source (exception message, or the server's own text for network failures).
/// </summary>
public sealed record BobError
{
    /// <summary>Short id to quote in a report, unique per classified error.</summary>
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public required BobErrorOrigin Origin { get; init; }
    /// <summary>Catalog code, e.g. <c>BOB-PARAM-001</c>. See error-catalog.json.</summary>
    public required string Code { get; init; }
    /// <summary>The exact message. Never rewritten or truncated.</summary>
    public required string Message { get; init; }
    /// <summary>Component that raised or received the error, when known.</summary>
    public string? Component { get; init; }
    /// <summary>Structured facts: parameter, passed, expected, path, status, url, script, service ...</summary>
    public IReadOnlyDictionary<string, string> Facts { get; init; } = new Dictionary<string, string>();
    /// <summary>HTTP status for network failures that got a response.</summary>
    public int? StatusCode { get; init; }
    public string? StackTrace { get; init; }
    /// <summary>Inner exceptions, outermost first.</summary>
    public IReadOnlyList<BobErrorChainItem> Inner { get; init; } = [];
    /// <summary>The original exception, when available. Not serialized.</summary>
    public Exception? Exception { get; init; }

    public string Badge => Origin.Badge();
}
