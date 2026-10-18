using System.Diagnostics.CodeAnalysis;
using BobsComponents.Errors;

namespace BobsComponents.Docs.Generation;

/// <summary>A demo page entry. The file <c>Demos/&lt;Type name&gt;.razor</c> is both the live demo and the code shown.</summary>
public sealed record DemoDoc(string Title, Type Type, string? Description = null)
{
    public string SourceName => Type.Name;
}

/// <summary>
/// The only hand-written facts about a documented component: where to find it. Everything else on its page
/// (summary, parameters, CSS, parts, DOM, errors, search entries) is generated from the code.
/// </summary>
/// <param name="Slug">URL segment under /bob/.</param>
/// <param name="Title">Heading and nav label.</param>
/// <param name="Component">The component type; its parameters are reflected and its summary is its <c>[Description]</c>.</param>
/// <param name="CssFile">Served path of its stylesheet, e.g. <c>css/components/modal.css</c>.</param>
/// <param name="Demos">Live demos, each a real .razor file in <c>Demos/</c>.</param>
/// <param name="Related">Service or helper types documented alongside it (public members listed).</param>
public sealed record ComponentDoc(
    string Slug,
    string Title,
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicEvents)]
    Type Component,
    string CssFile,
    IReadOnlyList<DemoDoc> Demos,
    IReadOnlyList<Type>? Related = null)
{
    public string Href => "bob/" + Slug;
}

public sealed record DemoSource(DemoDoc Demo, string Source);

public sealed record RelatedTypeDoc(string Name, string Summary, IReadOnlyList<MemberDoc> Members);

/// <summary>All generated facts for one component page.</summary>
public sealed class ComponentDocModel
{
    public required ComponentDoc Doc { get; init; }
    public required ComponentApi Api { get; init; }
    public required MarkupInfo Markup { get; init; }
    public required CssReference Css { get; init; }
    public required IReadOnlyList<DemoSource> Demos { get; init; }
    public required IReadOnlyList<RelatedTypeDoc> Related { get; init; }
    public required IReadOnlyList<ComponentDoc> Composes { get; init; }
    public required A11yPage? Accessibility { get; init; }
    public required IReadOnlyList<ErrorUse> Errors { get; init; }
    public required IReadOnlyList<ChangelogRelease> Changelog { get; init; }

    public static ComponentDocModel Build(ComponentDoc doc, IReadOnlyList<ComponentDoc> registry, BobErrorCatalog catalog)
    {
        var xml = XmlDocs.Library;
        var api = ApiReflector.Describe(doc.Component, xml);
        var markup = MarkupScanner.Scan(DocSources.ComponentMarkup(doc.Component.Name));
        var css = CssReference.Build(doc.CssFile, markup, TokenSheet.Library);

        var composes = registry
            .Where(r => r.Slug != doc.Slug && markup.ComponentTags.Contains(r.Component.Name))
            .ToList();

        var related = (doc.Related ?? [])
            .Select(t => new RelatedTypeDoc(t.Name, xml.Summary(t) ?? "", ApiReflector.Members(t, xml)))
            .ToList();

        var errors = ErrorSources.Derive(DocSources.ComponentMarkup(doc.Component.Name), catalog, doc.Component.Name, doc.Title).ToList();
        foreach (var composed in composes)
            foreach (var use in ErrorSources.Derive(DocSources.ComponentMarkup(composed.Component.Name), catalog, composed.Component.Name, composed.Title))
                if (errors.All(e => e.Entry.Code != use.Entry.Code)) errors.Add(use with { Via = composed.Title });
        errors = errors.OrderBy(e => e.Entry.Code, StringComparer.Ordinal).ToList();

        var a11yText = DocSources.Accessibility(doc.Slug);

        return new ComponentDocModel
        {
            Doc = doc,
            Api = api,
            Markup = markup,
            Css = css,
            Demos = doc.Demos.Select(d => new DemoSource(d, DocSources.DemoSource(d.SourceName))).ToList(),
            Related = related,
            Composes = composes,
            Accessibility = a11yText is null ? null : A11yNotesParser.Parse(a11yText),
            Errors = errors,
            Changelog = ChangelogParser.Parse(DocSources.Changelog()),
        };
    }
}
