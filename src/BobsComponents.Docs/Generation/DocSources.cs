using System.Reflection;

namespace BobsComponents.Docs.Generation;

/// <summary>
/// Everything the generators read, embedded in this assembly at build time from the real files: the library's
/// stylesheets, its component markup, the demo files, the XML doc file, accessibility notes and the changelog.
/// The browser build and the tests therefore read exactly the same text, and nothing is copied by hand.
/// </summary>
public static class DocSources
{
    private static readonly Assembly Asm = typeof(DocSources).Assembly;

    /// <summary>Stylesheet by its served path under <c>_content/BobsComponents/</c>, e.g. <c>css/components/modal.css</c>.</summary>
    public static string Css(string servedPath) => Read("src/wwwroot/" + servedPath.Replace('\\', '/'));

    /// <summary>Markup of a library component, e.g. <c>ModalSheet</c>.</summary>
    public static string ComponentMarkup(string componentName) => Read($"src/components/{componentName}.razor");

    public static bool HasComponentMarkup(string componentName) => Exists($"src/components/{componentName}.razor");

    /// <summary>A demo file exactly as written, e.g. <c>ModalSheetDemo</c>.</summary>
    public static string DemoSource(string demoTypeName) => Read($"src/demos/{demoTypeName}.razor");

    /// <summary>The library's compiler-generated XML documentation.</summary>
    public static string XmlDocs() => Read("src/BobsComponents.xml");

    public static string? Accessibility(string slug) => TryRead($"a11y/{slug}.md");

    public static string Changelog() => Read("docs/CHANGELOG.md");

    public static IEnumerable<string> Names(string prefix) =>
        Asm.GetManifestResourceNames().Where(n => n.StartsWith(prefix, StringComparison.Ordinal)).OrderBy(n => n, StringComparer.Ordinal);

    public static bool Exists(string logicalName) => Asm.GetManifestResourceNames().Contains(logicalName);

    public static string Read(string logicalName) =>
        TryRead(logicalName) ?? throw new InvalidOperationException(
            $"Docs source '{logicalName}' is not embedded in BobsComponents.Docs. Embedded: {string.Join(", ", Asm.GetManifestResourceNames().Take(40))}");

    public static string? TryRead(string logicalName)
    {
        using var stream = Asm.GetManifestResourceStream(logicalName);
        if (stream is null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
