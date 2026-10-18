using System.Text;
using System.Text.RegularExpressions;

namespace BobsComponents.Docs.Generation;

/// <summary>A styleable part found in a component's markup: its name, element and the bob- classes on that tag.</summary>
/// <param name="Name">The <c>data-bob-part</c> value.</param>
/// <param name="Element">The tag it sits on (a component tag such as <c>AsyncButton</c> is reported as written).</param>
/// <param name="Classes">Static <c>bob-*</c> class names written on that tag.</param>
/// <param name="ClassPrefixes">Class names built at run time, e.g. <c>bob-toast--</c> followed by an expression.</param>
public sealed record MarkupPart(string Name, string Element, IReadOnlyList<string> Classes, IReadOnlyList<string> ClassPrefixes);

public sealed record MarkupInfo(
    IReadOnlyList<MarkupPart> Parts,
    IReadOnlyList<string> Classes,
    IReadOnlyList<string> ClassPrefixes,
    IReadOnlyList<string> ComponentTags);

/// <summary>
/// Reads a component's <c>.razor</c> markup to find every <c>data-bob-part</c>, the element it is on and the
/// <c>bob-</c> classes written there. Razor comments are ignored, so documentation examples inside them are not parts.
/// </summary>
public static partial class MarkupScanner
{
    public static MarkupInfo Scan(string razor)
    {
        var text = CommentRegex().Replace(razor, " ");
        var parts = new List<MarkupPart>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (Match m in PartRegex().Matches(text))
        {
            var name = m.Groups[1].Value;
            if (!seen.Add(name)) continue;
            var (tagText, element) = TagAround(text, m.Index);
            var (classes, prefixes) = ClassTokens(tagText);
            parts.Add(new MarkupPart(name, element, classes, prefixes));
        }

        var (allClasses, allPrefixes) = ClassTokens(PartRegex().Replace(text, ""));
        var tags = ComponentTagRegex().Matches(text).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal).ToList();
        return new MarkupInfo(parts, allClasses, allPrefixes, tags);
    }

    /// <summary>Finds the start tag that contains the given offset (an attribute inside it) and its element name.</summary>
    private static (string Text, string Element) TagAround(string s, int offset)
    {
        var start = offset;
        while (start > 0 && !(s[start] == '<' && start + 1 < s.Length && char.IsLetter(s[start + 1]))) start--;

        var nameEnd = start + 1;
        while (nameEnd < s.Length && (char.IsLetterOrDigit(s[nameEnd]) || s[nameEnd] is '.' or '-' or ':')) nameEnd++;
        var element = s[(start + 1)..nameEnd];

        var i = nameEnd;
        var depth = 0;
        char quote = '\0';
        while (i < s.Length)
        {
            var c = s[i];
            if (depth > 0)
            {
                if (c == '(') depth++;
                else if (c == ')') depth--;
            }
            else if (quote != '\0')
            {
                if (c == quote) quote = '\0';
                else if (c == '(' && i > 0 && s[i - 1] == '@') depth++;
            }
            else
            {
                if (c is '"' or '\'') quote = c;
                else if (c == '(' && s[i - 1] == '@') depth++;
                else if (c == '>' && s[i - 1] != '=') break;
            }
            i++;
        }
        return (s[start..Math.Min(i + 1, s.Length)], element);
    }

    private static (List<string> Classes, List<string> Prefixes) ClassTokens(string text)
    {
        var classes = new List<string>();
        var prefixes = new List<string>();
        foreach (Match m in ClassRegex().Matches(text))
        {
            var token = m.Value;
            if (token.EndsWith('-') || token.EndsWith('_')) { if (!prefixes.Contains(token)) prefixes.Add(token); }
            else if (!classes.Contains(token)) classes.Add(token);
        }
        return (classes, prefixes);
    }

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline)] private static partial Regex CommentRegex();
    [GeneratedRegex("data-bob-part=\"([\\w-]+)\"")] private static partial Regex PartRegex();
    [GeneratedRegex(@"(?<![\w-])bob-[a-z0-9]+(?:[-_]+[a-z0-9]+)*[-_]*")] private static partial Regex ClassRegex();
    [GeneratedRegex(@"<([A-Z][A-Za-z0-9]*)[\s/>]")] private static partial Regex ComponentTagRegex();
}
