using System.Text.RegularExpressions;

namespace BobsComponents.Docs.Generation;

/// <summary>One accessibility behavior, the tests that prove it, or a recorded gap that has no test.</summary>
public sealed record A11yNote(string Text, IReadOnlyList<string> Tests, bool IsGap);

public sealed record A11yPage(string Title, string Source, IReadOnlyList<A11yNote> Notes);

/// <summary>
/// Parses <c>Content/accessibility/&lt;slug&gt;.md</c>: a heading, an optional "Seeded from" line and bullets.
/// A bullet ends with <c>[test: ClassName.Method, ...]</c> naming the xunit tests that prove it; a bullet that starts
/// with <c>gap:</c> records a known gap and needs no test. A test in the test project checks that every named test exists.
/// </summary>
public static partial class A11yNotesParser
{
    public static A11yPage Parse(string markdown)
    {
        var title = "";
        var source = "";
        var notes = new List<A11yNote>();
        var current = new List<string>();

        void Flush()
        {
            if (current.Count == 0) return;
            var text = string.Join(' ', current.Select(l => l.Trim())).Trim();
            current.Clear();
            var tests = new List<string>();
            var m = TestsRegex().Match(text);
            if (m.Success)
            {
                tests.AddRange(m.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                text = text.Remove(m.Index, m.Length).Trim();
            }
            var gap = text.StartsWith("gap:", StringComparison.OrdinalIgnoreCase);
            if (gap) text = text["gap:".Length..].Trim();
            notes.Add(new A11yNote(text, tests, gap));
        }

        foreach (var line in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            if (line.StartsWith("# ", StringComparison.Ordinal)) { Flush(); title = line[2..].Trim(); }
            else if (line.StartsWith("Seeded from", StringComparison.OrdinalIgnoreCase)) { Flush(); source = line.Trim(); }
            else if (line.StartsWith("- ", StringComparison.Ordinal)) { Flush(); current.Add(line[2..]); }
            else if (current.Count > 0 && line.StartsWith("  ", StringComparison.Ordinal)) current.Add(line);
            else if (string.IsNullOrWhiteSpace(line)) Flush();
        }
        Flush();
        return new A11yPage(title, source, notes);
    }

    [GeneratedRegex(@"\[tests?:\s*([^\]]+)\]")] private static partial Regex TestsRegex();
}
