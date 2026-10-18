using System.Text.RegularExpressions;

namespace BobsComponents.Docs.Generation;

public enum SearchKind { Page, Component, Parameter, Part, Class, Variable, Effect, Error }

/// <param name="Haystack">Lowercase text the query is matched against.</param>
public sealed record SearchEntry(SearchKind Kind, string Title, string Detail, string Component, string Href, string Haystack);

public sealed record SearchHit(SearchEntry Entry, int Score);

/// <summary>
/// One search over everything the generators know: components, parameters, parts, classes, variables, the effect of
/// each declaration ("border of sheet") and error codes. All words in the query must match (stop words are ignored);
/// if nothing matches all of them, entries matching any word are returned instead.
/// </summary>
public static partial class DocSearch
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        { "what", "which", "how", "does", "do", "i", "the", "a", "an", "of", "to", "is", "are", "change", "changes", "set", "sets", "for", "in", "on", "my", "can" };

    private static readonly Lazy<IReadOnlyList<SearchEntry>> Shared = new(() => BuildIndex(DocRegistry.Components, DocRegistry.ReferenceSheets));

    public static IReadOnlyList<SearchEntry> Index => Shared.Value;

    public static IReadOnlyList<SearchEntry> BuildIndex(IReadOnlyList<ComponentDoc> components, IReadOnlyList<ReferenceSheet> referenceSheets)
    {
        var entries = new List<SearchEntry>
        {
            Make(SearchKind.Page, "Changelog", "Breaking changes first, per version", "", "bob/changelog"),
            Make(SearchKind.Page, "Error catalog", "Every error code: origin, message, cause, fix", "", "bob/error-catalog"),
            Make(SearchKind.Page, "Error model", "Six origins, one card", "", "bob/errors"),
            Make(SearchKind.Page, "Dev kit", "Errors, network, interop and DI panels", "", "bob/dev-kit"),
        };

        foreach (var doc in components)
        {
            var model = DocRegistry.Model(doc);
            entries.Add(Make(SearchKind.Component, doc.Title, model.Api.Summary, doc.Title, doc.Href));
            AddParameters(entries, model, doc);
            AddCss(entries, model.Css, doc.Title, doc.Href);
        }

        foreach (var sheet in referenceSheets)
            AddCss(entries, DocRegistry.ReferenceFor(sheet), sheet.Title, sheet.Href);

        foreach (var error in DocRegistry.Catalog.All)
            entries.Add(Make(SearchKind.Error, error.Code + " " + error.Title, $"{error.Cause} {error.Fix}", string.Join(", ", error.Components),
                "bob/error-catalog#" + error.Code, error.Message, error.Origin.ToString()));

        return entries;
    }

    private static void AddParameters(List<SearchEntry> entries, ComponentDocModel model, ComponentDoc doc)
    {
        foreach (var p in model.Api.Parameters.Where(p => !p.Inherited))
            entries.Add(Make(SearchKind.Parameter, $"{doc.Title}.{p.Name}", $"{p.Type}. {p.Summary}", doc.Title, $"{doc.Href}#param-{p.Name}", p.Kind.ToString()));
    }

    private static void AddCss(List<SearchEntry> entries, CssReference css, string component, string href)
    {
        foreach (var part in css.Parts)
            entries.Add(Make(SearchKind.Part, $"{component} part: {part.Name}", $"data-bob-part=\"{part.Name}\" on <{part.Element}> {string.Join(' ', part.Classes.Select(c => "." + c))}",
                component, $"{href}#part-{part.Name}", string.Join(' ', part.Variables)));

        foreach (var cls in css.Classes)
            entries.Add(Make(SearchKind.Class, "." + cls.Name, $"{cls.Kind} class of {component}" + (cls.Parts.Count > 0 ? $", on part {string.Join(", ", cls.Parts)}" : ""),
                component, $"{href}#class-{cls.Name}"));

        foreach (var v in css.Variables)
            entries.Add(Make(SearchKind.Variable, v.Name, $"Default {v.Default}. {v.Effects}", component, $"{href}#var-{v.Name.TrimStart('-')}"));

        // One entry per (target, property, state): "what changes the border of the sheet" lands here.
        foreach (var group in css.Rules.SelectMany(r => r.Properties.Select(p => (Rule: r, Property: p)))
                     .GroupBy(x => (x.Rule.Target, x.Property, x.Rule.State)))
        {
            var vars = group.SelectMany(x => x.Rule.Variables).Distinct().ToList();
            var state = group.Key.State.Length == 0 ? "" : $" when {group.Key.State}";
            entries.Add(Make(SearchKind.Effect, $"{group.Key.Property} of {group.Key.Target}{state}",
                vars.Count == 0 ? "Fixed in the stylesheet; override with a selector." : "Set with " + string.Join(", ", vars),
                component, href + "#css", string.Join(' ', vars)));
        }
    }

    private static SearchEntry Make(SearchKind kind, string title, string detail, string component, string href, params string[] extra) =>
        new(kind, title, detail, component, href, string.Join(' ', new[] { title, detail, component }.Concat(extra)).ToLowerInvariant());

    public static IReadOnlyList<string> Tokenize(string query) =>
        TokenRegex().Matches(query.ToLowerInvariant()).Select(m => m.Value).Where(t => !StopWords.Contains(t)).Distinct().ToList();

    public static IReadOnlyList<SearchHit> Search(string query, int max = 60) => Search(Index, query, max);

    public static IReadOnlyList<SearchHit> Search(IReadOnlyList<SearchEntry> index, string query, int max = 60)
    {
        var tokens = Tokenize(query);
        if (tokens.Count == 0) return [];

        var all = Score(index, tokens, requireAll: true);
        if (all.Count == 0 && tokens.Count > 1) all = Score(index, tokens, requireAll: false);
        return all.OrderByDescending(h => h.Score).ThenBy(h => h.Entry.Title, StringComparer.Ordinal).Take(max).ToList();
    }

    private static List<SearchHit> Score(IReadOnlyList<SearchEntry> index, IReadOnlyList<string> tokens, bool requireAll)
    {
        var hits = new List<SearchHit>();
        foreach (var entry in index)
        {
            var matched = tokens.Count(t => entry.Haystack.Contains(t, StringComparison.Ordinal));
            if (matched == 0 || (requireAll && matched < tokens.Count)) continue;

            var title = entry.Title.ToLowerInvariant();
            var score = KindWeight(entry.Kind) + matched * 10;
            foreach (var t in tokens)
            {
                if (title == t || title == "." + t || title.EndsWith("." + t, StringComparison.Ordinal)) score += 120;
                else if (title.StartsWith(t, StringComparison.Ordinal)) score += 40;
                else if (title.Contains(t, StringComparison.Ordinal)) score += 20;
            }
            hits.Add(new SearchHit(entry, score));
        }
        return hits;
    }

    private static int KindWeight(SearchKind kind) => kind switch
    {
        SearchKind.Component => 100,
        SearchKind.Page => 90,
        SearchKind.Variable => 70,
        SearchKind.Part => 60,
        SearchKind.Class => 50,
        SearchKind.Parameter => 45,
        SearchKind.Effect => 40,
        SearchKind.Error => 30,
        _ => 0,
    };

    [GeneratedRegex(@"[a-z0-9_.\-]+")] private static partial Regex TokenRegex();
}
