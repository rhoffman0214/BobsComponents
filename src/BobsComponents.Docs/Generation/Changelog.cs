namespace BobsComponents.Docs.Generation;

public sealed record ChangelogSection(string Title, IReadOnlyList<string> Items)
{
    public bool IsBreaking => Title.Equals("Breaking", StringComparison.OrdinalIgnoreCase);
}

public sealed record ChangelogRelease(string Version, string? Date, IReadOnlyList<ChangelogSection> Sections)
{
    public IEnumerable<ChangelogSection> Breaking => Sections.Where(s => s.IsBreaking);
    public bool HasBreaking => Breaking.Any(s => s.Items.Count > 0);
}

/// <summary>
/// Parses docs/CHANGELOG.md: <c>## 1.2.3 - 2026-01-01</c> headings (newest first) with <c>### Breaking</c>,
/// <c>### Added</c> and so on. Within each release, Breaking is always listed first no matter where it was written.
/// </summary>
public static class ChangelogParser
{
    private static readonly string[] SectionOrder = ["Breaking", "Added", "Changed", "Fixed", "Removed", "Security", "Docs"];

    public static IReadOnlyList<ChangelogRelease> Parse(string markdown)
    {
        var releases = new List<ChangelogRelease>();
        string? version = null;
        string? date = null;
        var sections = new List<(string Title, List<string> Items)>();

        void Flush()
        {
            if (version is null) return;
            var ordered = sections
                .Select(s => new ChangelogSection(s.Title, s.Items))
                .OrderBy(s => OrderOf(s.Title))
                .ToList();
            releases.Add(new ChangelogRelease(version, date, ordered));
        }

        foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();
            if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                Flush();
                sections = [];
                var heading = line[3..].Trim();
                var dash = heading.IndexOf(" - ", StringComparison.Ordinal);
                version = dash < 0 ? heading : heading[..dash].Trim();
                date = dash < 0 ? null : heading[(dash + 3)..].Trim();
            }
            else if (line.StartsWith("### ", StringComparison.Ordinal) && version is not null)
            {
                sections.Add((line[4..].Trim(), []));
            }
            else if (line.StartsWith("- ", StringComparison.Ordinal) && sections.Count > 0)
            {
                sections[^1].Items.Add(line[2..].Trim());
            }
            else if (line.StartsWith("  ", StringComparison.Ordinal) && sections.Count > 0 && sections[^1].Items.Count > 0)
            {
                sections[^1].Items[^1] += " " + line.Trim();
            }
        }
        Flush();
        return releases;
    }

    private static int OrderOf(string title)
    {
        var i = Array.FindIndex(SectionOrder, s => s.Equals(title, StringComparison.OrdinalIgnoreCase));
        return i < 0 ? SectionOrder.Length : i;
    }
}
