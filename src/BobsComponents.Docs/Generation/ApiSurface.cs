namespace BobsComponents.Docs.Generation;

/// <summary>
/// The public names a consuming app can depend on, one per line, generated from the code: CSS classes, component
/// variables, parts, parameters and error codes. docs/api-surface.txt is the committed snapshot; a test fails when a
/// name in the snapshot disappears without a "Breaking" entry in the changelog.
/// </summary>
public static class ApiSurface
{
    public static IReadOnlyList<string> Lines()
    {
        var lines = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var doc in DocRegistry.Components)
        {
            var model = DocRegistry.Model(doc);
            lines.Add("component " + doc.Component.Name);
            foreach (var p in model.Api.Parameters.Where(p => !p.Inherited)) lines.Add($"param {doc.Component.Name}.{p.Name}");
            AddCss(lines, model.Css, doc.Component.Name);
        }

        foreach (var sheet in DocRegistry.ReferenceSheets)
        {
            lines.Add("component " + sheet.MarkupComponent);
            AddCss(lines, DocRegistry.ReferenceFor(sheet), sheet.MarkupComponent);
        }

        foreach (var error in DocRegistry.Catalog.All) lines.Add("error " + error.Code);
        return lines.ToList();
    }

    /// <summary>
    /// Compares a committed snapshot with the current surface. <c>Unexplained</c> are names that disappeared without being
    /// mentioned in the Breaking text; <c>Added</c> are names missing from the snapshot (it needs refreshing).
    /// </summary>
    public static (IReadOnlyList<string> Unexplained, IReadOnlyList<string> Added) Diff(
        IEnumerable<string> snapshot, IEnumerable<string> current, string breakingText)
    {
        var s = snapshot.Where(l => l.Length > 0).ToHashSet(StringComparer.Ordinal);
        var c = current.ToHashSet(StringComparer.Ordinal);
        var unexplained = s.Except(c)
            .Where(line => !breakingText.Contains(line[(line.IndexOf(' ') + 1)..], StringComparison.Ordinal))
            .OrderBy(l => l, StringComparer.Ordinal).ToList();
        var added = c.Except(s).OrderBy(l => l, StringComparer.Ordinal).ToList();
        return (unexplained, added);
    }

    private static void AddCss(SortedSet<string> lines, CssReference css, string component)
    {
        foreach (var c in css.Classes) lines.Add("class " + c.Name);
        foreach (var v in css.Variables) lines.Add("var " + v.Name);
        foreach (var p in css.Parts) lines.Add($"part {component}/{p.Name}");
    }
}
