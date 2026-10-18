using Bunit;
using BobsComponents.Components;
using BobsComponents.Docs;
using BobsComponents.Docs.Components;
using BobsComponents.Docs.Generation;
using BobsComponents.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BobsComponents.Tests.Docs;

/// <summary>Renders the generated pages and checks that what the generators know actually reaches the HTML.</summary>
public class DocsRenderTests : TestContext
{
    public DocsRenderTests()
    {
        Services.AddBobComponents();
        Services.AddBobDocs();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public static IEnumerable<object[]> Docs() => DocRegistry.Components.Select(c => new object[] { c.Slug });

    private IRenderedComponent<ComponentDocPage> RenderPage(string slug) =>
        RenderComponent<ComponentDocPage>(p => p.Add(c => c.Doc, DocRegistry.Find(slug)!));

    [Theory]
    [MemberData(nameof(Docs))]
    public void Page_ShowsTheTitleSummary_AndEverySection(string slug)
    {
        var doc = DocRegistry.Find(slug)!;
        var cut = RenderPage(slug);

        Assert.Equal(doc.Title, cut.Find("h1").TextContent);
        Assert.Contains(DocRegistry.Model(doc).Api.Summary.Replace("`", ""), cut.Find(".bobdoc-lead").TextContent);
        foreach (var id in new[] { "demo", "api", "css", "dom", "accessibility", "errors" })
            Assert.NotNull(cut.Find("#" + id));
        Assert.Empty(cut.FindAll(".bobdoc-missing"));
    }

    [Theory]
    [MemberData(nameof(Docs))]
    public void Page_ListsEveryParameter_Variable_Part_AndClass_AsAnchoredRows(string slug)
    {
        var doc = DocRegistry.Find(slug)!;
        var model = DocRegistry.Model(doc);
        var cut = RenderPage(slug);

        foreach (var p in model.Api.Parameters) Assert.NotNull(cut.Find("#" + DocIds.Param(p.Name)));
        foreach (var v in model.Css.Variables) Assert.NotNull(cut.Find("#" + DocIds.Var(v.Name)));
        foreach (var part in model.Css.Parts) Assert.NotNull(cut.Find("#" + DocIds.Part(part.Name)));
        foreach (var c in model.Css.Classes) Assert.NotNull(cut.Find("#" + DocIds.Class(c.Name)));
    }

    [Theory]
    [MemberData(nameof(Docs))]
    public void RequiredParameters_AreFlagged_AndTheShownCodeIsTheDemoFileItself(string slug)
    {
        var doc = DocRegistry.Find(slug)!;
        var model = DocRegistry.Model(doc);
        var cut = RenderPage(slug);

        var required = model.Api.Parameters.Count(p => p.Required);
        Assert.Equal(required, cut.FindAll("#api .bobdoc-required").Count);

        var shown = cut.FindAll(".bobdoc-frame__source pre code").Select(c => c.TextContent).ToList();
        Assert.Equal(doc.Demos.Select(d => DocSources.DemoSource(d.SourceName)), shown);
    }

    [Fact]
    public void DomMap_FollowsTheMarkupTheBrowserReports_AndFillsTheSitsInColumn()
    {
        var cut = RenderPage("modal-sheet");
        Assert.Contains("appears once the demo has rendered", cut.Find("#dom").TextContent);
        Assert.Contains("open the demo", cut.Find("#part-title").TextContent);

        const string html = """
            <div class="bob-modal-backdrop" data-bob-part="backdrop"><div class="bob-modal" data-bob-part="sheet" role="dialog">
            <div class="bob-modal__header" data-bob-part="header"><h3 class="bob-modal__title" data-bob-part="title">Edit item</h3></div></div></div>
            """;
        cut.InvokeAsync(() => cut.FindComponent<DemoFrame>().Instance.OnDomChanged(html));
        cut.WaitForAssertion(() =>
        {
            var rows = cut.FindAll("#dom .bobdoc-dom__row");
            Assert.Equal(4, rows.Count);
            Assert.Contains("title", rows[3].TextContent);
            Assert.Contains("bob-modal__title", rows[3].TextContent);
            Assert.Contains("in header", cut.Find("#part-title").TextContent);
            Assert.Contains("in backdrop", cut.Find("#part-sheet").TextContent);
        });
    }

    [Fact]
    public void ErrorsSection_LinksEachCodeToItsCatalogEntry_AndSaysWhy()
    {
        var cut = RenderPage("modal-sheet");
        var links = cut.FindAll("#errors a[href^='bob/error-catalog#']").Select(a => a.GetAttribute("href")).ToList();
        Assert.Contains("bob/error-catalog#BOB-ASSET-001", links);
        Assert.Contains("bob/error-catalog#BOB-PARAM-002", links);
        Assert.Contains("focusTrap.js", cut.Find("#errors").TextContent);
    }

    [Fact]
    public void Accessibility_ShowsEachNote_WithTheTestsThatProveIt()
    {
        var cut = RenderPage("modal-sheet");
        var chips = cut.FindAll("#accessibility .bobdoc-chip--test").Select(c => c.TextContent).ToList();
        Assert.Contains("ModalSheetTests.StackedSheets_EachGetTheirOwnTrap", chips);
        Assert.Contains("Known gap", cut.Find("#accessibility .bobdoc-a11y__gap").TextContent);
    }

    [Fact]
    public void CopyButtons_ExistForVariablesPartsAndTheWholeBlocks()
    {
        var cut = RenderPage("modal-sheet");
        Assert.NotEmpty(cut.FindAll("#var-bob-modal-bg button[aria-label='Copy --bob-modal-bg declaration']"));
        Assert.NotEmpty(cut.FindAll("#part-sheet button"));
        Assert.Contains(cut.FindAll("#css-snippets ~ .bobdoc-snippets pre code"), c => c.TextContent.Contains(":root {") && c.TextContent.Contains("--bob-modal-bg: var(--bob-color-surface);"));
    }

    [Fact]
    public void ToastsPage_ListsTheServiceMembers()
    {
        var cut = RenderPage("toasts");
        Assert.Contains("Show(string text", cut.Find("#type-IBobToastService ~ .bobdoc-tablewrap").TextContent);
    }

    [Fact]
    public void ErrorCatalog_HasAnArticleForEveryCode_WhereTheCardLinkLands()
    {
        var catalog = DocRegistry.Catalog;
        var cut = RenderComponent<ErrorCatalogView>(p => p.Add(c => c.Catalog, catalog));

        foreach (var entry in catalog.All) Assert.NotNull(cut.Find("article#" + entry.Code));
        Assert.Contains("Message", cut.Find("article#BOB-PARAM-001").TextContent);
        Assert.Contains(cut.FindAll("article#BOB-ASSET-001 a.bobdoc-chip"), a => a.TextContent == "ModalSheet");
    }

    [Fact]
    public void ErrorCard_CodeLink_PointsAtAnEntryTheCatalogPageRenders()
    {
        // The site sets CatalogBaseUrl to the catalog page; the card links to "<base>#<code>".
        const string baseUrl = "bob/error-catalog";
        var error = new BobErrorClassifier().Classify(BobParameterException.Missing("Thing", "Size", "a number"));
        using var other = new TestContext();
        other.Services.AddBobComponents(o => o.Errors.CatalogBaseUrl = baseUrl);
        var card = other.RenderComponent<BobErrorCard>(p => p.Add(c => c.Error, error));
        var href = card.Find("a[data-bob-part='code']").GetAttribute("href")!;

        Assert.Equal($"{baseUrl}#{error.Code}", href);
        var page = RenderComponent<ErrorCatalogView>(p => p.Add(c => c.Catalog, DocRegistry.Catalog));
        Assert.NotNull(page.Find("article#" + href[(href.IndexOf('#') + 1)..]));
    }

    [Fact]
    public void Changelog_ShowsBreakingFirst_AndSaysWhenThereIsNone()
    {
        var releases = ChangelogParser.Parse("""
            ## 0.2.0 - 2026-11-01
            ### Added
            - New thing
            ### Breaking
            - Renamed `--bob-a`

            ## 0.1.0 - 2026-10-18
            ### Added
            - First
            """);
        var cut = RenderComponent<ChangelogView>(p => p.Add(c => c.Releases, releases));

        var first = cut.FindAll("section")[0];
        Assert.Equal("Breaking", first.QuerySelectorAll("h3")[0].TextContent);
        Assert.Contains("--bob-a", first.QuerySelector(".bobdoc-breaking")!.TextContent);
        Assert.Contains("No breaking changes", cut.FindAll("section")[1].TextContent);
    }

    [Fact]
    public void RealChangelog_Renders_AndTheNewestReleaseIsTheLibraryVersion()
    {
        var cut = RenderComponent<ChangelogView>();
        var newest = ChangelogParser.Parse(DocSources.Changelog())[0];
        Assert.Equal(typeof(LoadingSpinner).Assembly.GetName().Version!.ToString(3), newest.Version);
        Assert.NotEmpty(cut.FindAll("section"));
    }

    [Fact]
    public void SearchView_ShowsHitsWithKindAndLink_AndExplainsNoResults()
    {
        var hits = RenderComponent<DocSearchView>(p => p.Add(c => c.Query, "backdrop"));
        Assert.Contains(hits.FindAll(".bobdoc-result a"), a => a.GetAttribute("href") == "bob/modal-sheet#part-backdrop");
        Assert.NotEmpty(hits.FindAll(".bobdoc-kind--part"));

        var none = RenderComponent<DocSearchView>(p => p.Add(c => c.Query, "qqqqzzzz"));
        Assert.Contains("Nothing matches", none.Markup);
    }
}
