using System.Text;
using System.Text.RegularExpressions;

namespace BobsComponents.Docs.Generation;

/// <summary>One <c>var(--name, fallback)</c> read found in a declaration value.</summary>
public sealed record VarRef(string Name, string? Fallback);

public sealed record CssDeclaration(string Property, string Value, bool Important)
{
    /// <summary>Custom properties read by this value, outermost first, including ones nested in fallbacks.</summary>
    public IReadOnlyList<VarRef> VarRefs { get; } = CssParser.ParseVarRefs(Value);

    public bool IsCustomPropertyDeclaration => Property.StartsWith("--", StringComparison.Ordinal);
}

/// <summary>One compound selector, e.g. <c>.bob-modal[data-bob-part="sheet"]:hover</c>.</summary>
public sealed record CompoundSelector(
    string Text,
    string? Tag,
    IReadOnlyList<string> Classes,
    IReadOnlyList<string> Attributes,
    IReadOnlyList<string> Pseudos,
    string? Part,
    string Combinator);

public sealed record SelectorInfo(string Text, IReadOnlyList<CompoundSelector> Compounds)
{
    /// <summary>The compound that identifies the styled element (the last one).</summary>
    public CompoundSelector Subject => Compounds[^1];
}

/// <summary>A rule with its selectors, declarations and the at-rule conditions it sits inside.</summary>
public sealed record CssRule(
    string SelectorText,
    IReadOnlyList<SelectorInfo> Selectors,
    IReadOnlyList<CssDeclaration> Declarations,
    string? Condition);

public sealed record CssSheet(string File, IReadOnlyList<CssRule> Rules, IReadOnlyList<string> Keyframes, IReadOnlyList<string> Imports);

/// <summary>
/// A small CSS reader for the library's own stylesheets: rules, nested <c>@media</c>/<c>@layer</c>/<c>@supports</c>,
/// <c>@keyframes</c> names, declarations, selectors and <c>var()</c> reads. It is not a general-purpose parser; it
/// understands the plain, un-nested CSS this library is allowed to contain.
/// </summary>
public static partial class CssParser
{
    public static CssSheet Parse(string file, string css)
    {
        var text = StripComments(css);
        var rules = new List<CssRule>();
        var keyframes = new List<string>();
        var imports = new List<string>();
        var pos = 0;
        ParseBlock(text, ref pos, null, rules, keyframes, imports, topLevel: true);
        return new CssSheet(file, rules, keyframes, imports);
    }

    public static string StripComments(string css) => CommentRegex().Replace(css, "");

    private static void ParseBlock(string s, ref int pos, string? condition, List<CssRule> rules, List<string> keyframes, List<string> imports, bool topLevel)
    {
        while (pos < s.Length)
        {
            SkipWhitespace(s, ref pos);
            if (pos >= s.Length) return;
            if (s[pos] == '}') { pos++; if (!topLevel) return; continue; }

            var preludeStart = pos;
            var depth = 0;
            while (pos < s.Length && !(depth == 0 && (s[pos] == '{' || s[pos] == ';' || s[pos] == '}')))
            {
                if (s[pos] == '(') depth++;
                else if (s[pos] == ')') depth--;
                else if (s[pos] is '"' or '\'') pos = SkipString(s, pos);
                pos++;
            }
            var prelude = s[preludeStart..Math.Min(pos, s.Length)].Trim();
            if (pos >= s.Length) return;

            if (s[pos] == ';')
            {
                pos++;
                if (prelude.StartsWith("@import", StringComparison.OrdinalIgnoreCase)) imports.Add(prelude["@import".Length..].Trim());
                continue;
            }
            if (s[pos] == '}') continue;

            // s[pos] == '{'
            pos++;
            if (prelude.StartsWith('@'))
            {
                var name = AtRuleName(prelude);
                if (name is "media" or "supports" or "layer" or "container")
                {
                    var inner = name == "layer" ? condition : Combine(condition, prelude);
                    ParseBlock(s, ref pos, inner, rules, keyframes, imports, topLevel: false);
                }
                else
                {
                    if (name == "keyframes") keyframes.Add(prelude["@keyframes".Length..].Trim());
                    SkipBlock(s, ref pos);
                }
                continue;
            }

            var bodyStart = pos;
            SkipBlock(s, ref pos);
            var body = s[bodyStart..Math.Max(bodyStart, pos - 1)];
            rules.Add(new CssRule(prelude, ParseSelectorList(prelude), ParseDeclarations(body), condition));
        }
    }

    private static string AtRuleName(string prelude)
    {
        var m = AtNameRegex().Match(prelude);
        return m.Success ? m.Groups[1].Value.ToLowerInvariant() : "";
    }

    private static string Combine(string? outer, string inner) => outer is null ? inner : outer + " > " + inner;

    private static void SkipWhitespace(string s, ref int pos)
    {
        while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
    }

    private static int SkipString(string s, int pos)
    {
        var quote = s[pos++];
        while (pos < s.Length && s[pos] != quote) pos++;
        return pos;
    }

    /// <summary>Advances past the matching close brace; pos is left just after it.</summary>
    private static void SkipBlock(string s, ref int pos)
    {
        var depth = 1;
        while (pos < s.Length && depth > 0)
        {
            if (s[pos] == '{') depth++;
            else if (s[pos] == '}') depth--;
            else if (s[pos] is '"' or '\'') pos = SkipString(s, pos);
            pos++;
        }
    }

    public static IReadOnlyList<CssDeclaration> ParseDeclarations(string body)
    {
        var result = new List<CssDeclaration>();
        foreach (var part in SplitTopLevel(body, ';'))
        {
            var colon = part.IndexOf(':');
            if (colon <= 0) continue;
            var prop = part[..colon].Trim();
            var value = part[(colon + 1)..].Trim();
            if (prop.Length == 0 || value.Length == 0) continue;
            var important = false;
            if (value.EndsWith("!important", StringComparison.OrdinalIgnoreCase))
            {
                important = true;
                value = value[..^"!important".Length].Trim();
            }
            result.Add(new CssDeclaration(prop, value, important));
        }
        return result;
    }

    /// <summary>Splits on a separator that is not inside parentheses, brackets or quotes.</summary>
    public static IEnumerable<string> SplitTopLevel(string text, char separator)
    {
        var sb = new StringBuilder();
        int paren = 0, bracket = 0;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (c is '"' or '\'')
            {
                var end = Math.Min(SkipString(text, i), text.Length - 1);
                sb.Append(text, i, end - i + 1);
                i = end;
                continue;
            }
            if (c == '(') paren++;
            else if (c == ')') paren--;
            else if (c == '[') bracket++;
            else if (c == ']') bracket--;
            if (c == separator && paren == 0 && bracket == 0)
            {
                yield return sb.ToString();
                sb.Clear();
                continue;
            }
            sb.Append(c);
        }
        if (sb.Length > 0) yield return sb.ToString();
    }

    public static IReadOnlyList<SelectorInfo> ParseSelectorList(string prelude)
    {
        var list = new List<SelectorInfo>();
        foreach (var raw in SplitTopLevel(prelude, ','))
        {
            var text = raw.Trim();
            if (text.Length == 0) continue;
            list.Add(new SelectorInfo(text, ParseCompounds(text)));
        }
        return list;
    }

    private static List<CompoundSelector> ParseCompounds(string selector)
    {
        var compounds = new List<CompoundSelector>();
        var sb = new StringBuilder();
        var combinator = "";
        int paren = 0, bracket = 0;

        void Flush()
        {
            if (sb.Length == 0) return;
            compounds.Add(ParseCompound(sb.ToString(), combinator));
            sb.Clear();
            combinator = "";
        }

        foreach (var c in selector)
        {
            if (c == '(') paren++;
            else if (c == ')') paren--;
            else if (c == '[') bracket++;
            else if (c == ']') bracket--;

            if (paren == 0 && bracket == 0 && (char.IsWhiteSpace(c) || c is '>' or '+' or '~'))
            {
                Flush();
                if (c is '>' or '+' or '~') combinator = c.ToString();
                else if (combinator.Length == 0 && compounds.Count > 0) combinator = " ";
                continue;
            }
            sb.Append(c);
        }
        Flush();
        if (compounds.Count == 0) compounds.Add(ParseCompound(selector, ""));
        return compounds;
    }

    private static CompoundSelector ParseCompound(string text, string combinator)
    {
        var attributes = AttributeRegex().Matches(text).Select(m => m.Groups[1].Value).ToList();
        var withoutAttrs = AttributeRegex().Replace(text, "");
        var pseudos = PseudoRegex().Matches(withoutAttrs).Select(m => m.Value).ToList();
        var withoutPseudos = PseudoRegex().Replace(withoutAttrs, "");
        var classes = ClassRegex().Matches(withoutPseudos).Select(m => m.Groups[1].Value).ToList();
        var tagMatch = TagRegex().Match(withoutPseudos);
        string? part = null;
        foreach (var attr in attributes)
        {
            var m = PartAttrRegex().Match(attr);
            if (m.Success) part = m.Groups[1].Value;
        }
        return new CompoundSelector(text, tagMatch.Success ? tagMatch.Value : null, classes, attributes, pseudos, part, combinator);
    }

    /// <summary>All <c>var()</c> reads in a value, outermost first, then those nested in fallbacks.</summary>
    public static IReadOnlyList<VarRef> ParseVarRefs(string value)
    {
        var result = new List<VarRef>();
        CollectVarRefs(value, result);
        return result;
    }

    private static void CollectVarRefs(string value, List<VarRef> into)
    {
        var index = 0;
        while ((index = value.IndexOf("var(", index, StringComparison.Ordinal)) >= 0)
        {
            var start = index + 4;
            var depth = 1;
            var i = start;
            while (i < value.Length && depth > 0)
            {
                if (value[i] == '(') depth++;
                else if (value[i] == ')') depth--;
                i++;
            }
            var inner = value[start..Math.Max(start, i - 1)];
            var comma = FirstTopLevelComma(inner);
            var name = (comma < 0 ? inner : inner[..comma]).Trim();
            var fallback = comma < 0 ? null : inner[(comma + 1)..].Trim();
            if (name.StartsWith("--", StringComparison.Ordinal))
            {
                into.Add(new VarRef(name, fallback));
                if (fallback is not null) CollectVarRefs(fallback, into);
            }
            index = i;
        }
    }

    private static int FirstTopLevelComma(string text)
    {
        var depth = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '(') depth++;
            else if (text[i] == ')') depth--;
            else if (text[i] == ',' && depth == 0) return i;
        }
        return -1;
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)] private static partial Regex CommentRegex();
    [GeneratedRegex(@"^@([\w-]+)")] private static partial Regex AtNameRegex();
    [GeneratedRegex(@"\[([^\]]+)\]")] private static partial Regex AttributeRegex();
    [GeneratedRegex(@"::?[\w-]+(\([^)]*\))?")] private static partial Regex PseudoRegex();
    [GeneratedRegex(@"\.(-?[_a-zA-Z][\w-]*)")] private static partial Regex ClassRegex();
    [GeneratedRegex(@"^[a-zA-Z][\w-]*")] private static partial Regex TagRegex();
    [GeneratedRegex("data-bob-part\\s*=\\s*[\"']?([\\w-]+)")] private static partial Regex PartAttrRegex();
}
