using System.Net;
using System.Text;

namespace BobsComponents.Docs.Generation;

/// <summary>One element of a rendered DOM tree.</summary>
public sealed class DomNode
{
    public required string Tag { get; init; }
    public List<KeyValuePair<string, string>> Attributes { get; } = [];
    public List<DomNode> Children { get; } = [];
    public StringBuilder TextBuilder { get; } = new();
    public DomNode? Parent { get; set; }

    public string Text => TextBuilder.ToString().Trim();

    public string? Attr(string name) => Attributes.FirstOrDefault(a => a.Key == name).Value;

    public IReadOnlyList<string> Classes =>
        (Attr("class") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

/// <summary>A tiny HTML reader for serialized DOM (what <c>outerHTML</c> gives): elements, attributes, text, comments.</summary>
public static class HtmlTree
{
    private static readonly HashSet<string> Void = new(StringComparer.OrdinalIgnoreCase)
        { "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr" };

    private static readonly HashSet<string> RawText = new(StringComparer.OrdinalIgnoreCase) { "script", "style" };

    /// <summary>Parses markup into a synthetic root whose children are the top-level elements.</summary>
    public static DomNode Parse(string html)
    {
        var root = new DomNode { Tag = "#root" };
        var current = root;
        var i = 0;
        while (i < html.Length)
        {
            if (html[i] != '<')
            {
                var next = html.IndexOf('<', i);
                if (next < 0) next = html.Length;
                current.TextBuilder.Append(WebUtility.HtmlDecode(html[i..next]));
                i = next;
                continue;
            }

            if (html.AsSpan(i).StartsWith("<!--"))
            {
                var end = html.IndexOf("-->", i + 4, StringComparison.Ordinal);
                i = end < 0 ? html.Length : end + 3;
                continue;
            }
            if (i + 1 < html.Length && (html[i + 1] == '!' || html[i + 1] == '?'))
            {
                var end = html.IndexOf('>', i);
                i = end < 0 ? html.Length : end + 1;
                continue;
            }
            if (i + 1 < html.Length && html[i + 1] == '/')
            {
                var end = html.IndexOf('>', i);
                var name = html[(i + 2)..(end < 0 ? html.Length : end)].Trim();
                // Close the nearest open element with this name; ignore stray closers.
                for (var n = current; n is not null && n.Tag != "#root"; n = n.Parent)
                {
                    if (string.Equals(n.Tag, name, StringComparison.OrdinalIgnoreCase)) { current = n.Parent ?? root; break; }
                }
                i = end < 0 ? html.Length : end + 1;
                continue;
            }
            if (i + 1 < html.Length && !char.IsLetter(html[i + 1]))
            {
                current.TextBuilder.Append('<');
                i++;
                continue;
            }

            i = ReadStartTag(html, i, out var node, out var selfClosing);
            node.Parent = current;
            current.Children.Add(node);
            if (RawText.Contains(node.Tag))
            {
                var end = html.IndexOf("</" + node.Tag, i, StringComparison.OrdinalIgnoreCase);
                if (end < 0) { i = html.Length; continue; }
                var close = html.IndexOf('>', end);
                i = close < 0 ? html.Length : close + 1;
                continue;
            }
            if (!selfClosing && !Void.Contains(node.Tag)) current = node;
        }
        return root;
    }

    private static int ReadStartTag(string s, int start, out DomNode node, out bool selfClosing)
    {
        var i = start + 1;
        var nameStart = i;
        while (i < s.Length && !char.IsWhiteSpace(s[i]) && s[i] != '>' && s[i] != '/') i++;
        node = new DomNode { Tag = s[nameStart..i].ToLowerInvariant() };
        selfClosing = false;

        while (i < s.Length)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
            if (i >= s.Length) break;
            if (s[i] == '>') return i + 1;
            if (s[i] == '/') { selfClosing = true; i++; continue; }

            var attrStart = i;
            while (i < s.Length && !char.IsWhiteSpace(s[i]) && s[i] is not ('=' or '>' or '/')) i++;
            var name = s[attrStart..i];
            var value = "";
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
            if (i < s.Length && s[i] == '=')
            {
                i++;
                while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
                if (i < s.Length && s[i] is '"' or '\'')
                {
                    var quote = s[i++];
                    var end = s.IndexOf(quote, i);
                    if (end < 0) end = s.Length;
                    value = s[i..end];
                    i = Math.Min(end + 1, s.Length);
                }
                else
                {
                    var valStart = i;
                    while (i < s.Length && !char.IsWhiteSpace(s[i]) && s[i] != '>') i++;
                    value = s[valStart..i];
                }
            }
            if (name.Length > 0) node.Attributes.Add(new(name, WebUtility.HtmlDecode(value)));
        }
        return s.Length;
    }
}

/// <summary>One row of the DOM map: an element with its depth, classes, part and the notable attributes.</summary>
public sealed record DomRow(
    int Index,
    int Depth,
    string Tag,
    IReadOnlyList<string> BobClasses,
    IReadOnlyList<string> OtherClasses,
    string? Part,
    string? ParentPart,
    IReadOnlyList<string> Attributes,
    string Text)
{
    public bool IsBob => Part is not null || BobClasses.Count > 0;
}

/// <summary>
/// Turns a rendered demo into the labelled tree shown on a docs page. <see cref="DomRow.Index"/> counts every element
/// in document order, skipping the insides of <c>&lt;svg&gt;</c>; the page's script uses the same rule to highlight the
/// matching element in the live demo.
/// </summary>
public static class DomMap
{
    private static readonly string[] NotableAttributes =
        ["id", "role", "type", "disabled", "tabindex", "href", "for", "autofocus", "data-bob-origin", "data-bob-code"];

    public static IReadOnlyList<DomRow> Build(string html)
    {
        var root = HtmlTree.Parse(html);
        var rows = new List<DomRow>();
        var index = 0;
        foreach (var child in root.Children) Walk(child, 0, null, rows, ref index);
        return rows;
    }

    private static void Walk(DomNode node, int depth, string? parentPart, List<DomRow> rows, ref int index)
    {
        var part = node.Attr("data-bob-part");
        var classes = node.Classes;
        var attrs = node.Attributes
            .Where(a => a.Key.StartsWith("aria-", StringComparison.Ordinal) || NotableAttributes.Contains(a.Key))
            .Select(a => a.Value.Length == 0 ? a.Key : $"{a.Key}={a.Value}")
            .ToList();

        rows.Add(new DomRow(
            index++,
            depth,
            node.Tag,
            classes.Where(c => c.StartsWith("bob-", StringComparison.Ordinal)).ToList(),
            classes.Where(c => !c.StartsWith("bob-", StringComparison.Ordinal)).ToList(),
            part,
            parentPart,
            attrs,
            Shorten(node.Text)));

        if (node.Tag == "svg") return;
        foreach (var child in node.Children) Walk(child, depth + 1, part ?? parentPart, rows, ref index);
    }

    private static string Shorten(string text)
    {
        var collapsed = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        return collapsed.Length <= 60 ? collapsed : collapsed[..57] + "...";
    }

    /// <summary>Where a part sits: the distinct names of the parts it is directly nested in across the given rows.</summary>
    public static IReadOnlyList<string> Placement(IEnumerable<DomRow> rows, string part) =>
        rows.Where(r => r.Part == part).Select(r => r.ParentPart ?? "(top level)").Distinct().ToList();
}
