using System.Text.RegularExpressions;
using BobsComponents.Errors;

namespace BobsComponents.Docs.Generation;

/// <summary>A catalog entry a component can raise, with the reason found in its source.</summary>
public sealed record ErrorUse(BobErrorCatalogEntry Entry, string Because, string? Via = null);

/// <summary>
/// Finds which catalog codes a component can raise by reading its source: <c>check.Required</c> raises BOB-PARAM-001,
/// <c>check.Range/Must/Defined</c> BOB-PARAM-002, <c>check.NotBoth</c> BOB-PARAM-003, <c>BobJs</c> BOB-ASSET-001,
/// <c>RequireService</c> BOB-ASSET-002 and <c>InvokeCallbackAsync</c> BOB-CB-001. Catalog entries that list the
/// component in <c>components</c> are added too (for example network codes). Components that compose others
/// inherit their codes, marked "via".
/// </summary>
public static partial class ErrorSources
{
    public static IReadOnlyList<ErrorUse> Derive(string razor, BobErrorCatalog catalog, string componentName, string title)
    {
        var text = CommentRegex().Replace(razor, " ");
        var uses = new List<ErrorUse>();

        void Add(string code, string because)
        {
            var entry = catalog.Get(code);
            if (entry is not null && uses.All(u => u.Entry.Code != code)) uses.Add(new ErrorUse(entry, because));
        }

        var required = Names(text, "Required");
        if (required.Count > 0) Add("BOB-PARAM-001", $"{Join(required)} {(required.Count == 1 ? "is" : "are")} required.");

        var invalid = Names(text, "Range", "Must", "Defined");
        if (invalid.Count > 0) Add("BOB-PARAM-002", $"{Join(invalid)} {(invalid.Count == 1 ? "is" : "are")} validated and can be rejected.");

        var conflicts = Names(text, "NotBoth");
        if (conflicts.Count > 0) Add("BOB-PARAM-003", $"{Join(conflicts)} cannot be combined with another parameter.");

        var scripts = BobJsRegex().Matches(text).Select(m => m.Groups[1].Value).Distinct().ToList();
        if (scripts.Count > 0) Add("BOB-ASSET-001", $"It loads {string.Join(", ", scripts.Select(s => $"`js/{ScriptFile(s)}`"))}; the message names the path if it is missing.");

        var requireCalls = ServiceRegex().Matches(text).Select(m => m.Groups[1].Value).Distinct().ToList();
        if (requireCalls.Count > 0) Add("BOB-ASSET-002", $"It needs {Join(requireCalls)} registered in DI.");

        var callbacks = CallbackRegex().Matches(text).Select(m => m.Groups[1].Value).Distinct().ToList();
        if (callbacks.Count > 0) Add("BOB-CB-001", $"Your {Join(callbacks)} handler{(callbacks.Count == 1 ? "" : "s")} can throw; the exception is wrapped and shown on a card.");

        foreach (var entry in catalog.All.Where(e => e.Components.Contains(componentName, StringComparer.Ordinal) || e.Components.Contains(title, StringComparer.Ordinal)))
            if (uses.All(u => u.Entry.Code != entry.Code)) uses.Add(new ErrorUse(entry, "Listed for this component in the catalog."));

        return uses.OrderBy(u => u.Entry.Code, StringComparer.Ordinal).ToList();
    }

    private static List<string> Names(string text, params string[] methods)
    {
        var pattern = $@"check\.(?:{string.Join("|", methods)})(?:<[^>]+>)?\(\s*nameof\((\w+)\)";
        return Regex.Matches(text, pattern).Select(m => m.Groups[1].Value).Distinct().ToList();
    }

    private static string Join(IReadOnlyList<string> names) => string.Join(", ", names.Select(n => $"`{n}`"));

    private static string ScriptFile(string constant) => constant switch
    {
        "FocusTrap" => "focusTrap.js",
        "ModalHistory" => "modalHistory.js",
        _ => constant,
    };

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline)] private static partial Regex CommentRegex();
    [GeneratedRegex(@"BobJs\.(?:ImportAsync|CallAsync)\([^;]*?BobJs\.(\w+)")] private static partial Regex BobJsRegex();
    [GeneratedRegex(@"RequireService<(\w+)>")] private static partial Regex ServiceRegex();
    [GeneratedRegex(@"InvokeCallbackAsync\(\s*nameof\((\w+)\)")] private static partial Regex CallbackRegex();
}
