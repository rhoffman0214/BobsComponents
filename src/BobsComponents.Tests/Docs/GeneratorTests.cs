using BobsComponents.Components;
using BobsComponents.Docs.Generation;
using BobsComponents.Services;
using Microsoft.AspNetCore.Components;

namespace BobsComponents.Tests.Docs;

public class CssParserTests
{
    private const string Sample = """
        /* header comment with .fake-class { } */
        @import url("x.css") layer(bob);
        .bob-a, .bob-b > .bob-b__c:hover {
            color: var(--bob-a-color, var(--bob-color-text, #000));
            padding: 1px 2px !important;
        }
        @media (min-width: 640px) {
            .bob-a[data-bob-part="root"] { border-radius: var(--bob-a-radius, 1rem); }
        }
        @keyframes bob-spin { from { opacity: 0; } to { opacity: 1; } }
        .bob-d { --bob-a-color: red; background: url("a;b.png"); }
        """;

    [Fact]
    public void Parses_Rules_Selectors_AndDeclarations_IgnoringCommentsAndKeyframeBodies()
    {
        var sheet = CssParser.Parse("x.css", Sample);

        Assert.Equal(["bob-spin"], sheet.Keyframes);
        Assert.Equal(3, sheet.Rules.Count);

        var first = sheet.Rules[0];
        Assert.Equal(2, first.Selectors.Count);
        var second = first.Selectors[1];
        Assert.Equal(["bob-b", "bob-b__c"], second.Compounds.SelectMany(c => c.Classes).ToArray());
        Assert.Equal(["bob-b__c"], second.Subject.Classes);
        Assert.Equal([":hover"], second.Subject.Pseudos);
        Assert.Equal(">", second.Subject.Combinator);
        Assert.True(first.Declarations[1].Important);
    }

    [Fact]
    public void KeepsMediaCondition_AndPartAttribute()
    {
        var rule = CssParser.Parse("x.css", Sample).Rules[1];
        Assert.Equal("@media (min-width: 640px)", rule.Condition);
        Assert.Equal("root", rule.Selectors[0].Subject.Part);
        Assert.Equal(["bob-a"], rule.Selectors[0].Subject.Classes);
    }

    [Fact]
    public void FindsNestedVarReads_WithFallbacks()
    {
        var refs = CssParser.ParseVarRefs("var(--bob-a-color, var(--bob-color-text, #000))");
        Assert.Equal(["--bob-a-color", "--bob-color-text"], refs.Select(r => r.Name).ToArray());
        Assert.Equal("var(--bob-color-text, #000)", refs[0].Fallback);
        Assert.Equal("#000", refs[1].Fallback);
    }

    [Fact]
    public void SemicolonInsideUrl_DoesNotSplitDeclarations()
    {
        var rule = CssParser.Parse("x.css", Sample).Rules[2];
        Assert.Equal(["--bob-a-color", "background"], rule.Declarations.Select(d => d.Property).ToArray());
        Assert.True(rule.Declarations[0].IsCustomPropertyDeclaration);
    }

    [Fact]
    public void EveryLibrarySheet_Parses_WithNoLostRules()
    {
        // Each sheet ends up with as many rules as it has selector blocks outside @keyframes.
        foreach (var name in DocSources.Names("src/wwwroot/css/"))
        {
            var css = CssParser.StripComments(DocSources.Read(name));
            var sheet = CssParser.Parse(name, css);
            var blocks = System.Text.RegularExpressions.Regex.Matches(css, @"\{").Count;
            Assert.True(sheet.Rules.Count > 0, name);
            Assert.True(sheet.Rules.Count <= blocks, name);
        }
    }
}

public class MarkupScannerTests
{
    [Fact]
    public void FindsParts_WithElementAndClasses_AndIgnoresRazorComments()
    {
        const string razor = """
            @* data-bob-part="fake" in a comment must not count *@
            <div class="bob-x @Extra" data-bob-part="root" @onclick="() => Go()">
                <span class="bob-x__label @(Hidden ? "bob-visually-hidden" : "")" data-bob-part="label">t</span>
                <div class="bob-x--@Level.ToString()" data-bob-part="level"></div>
            </div>
            <Thing CssClass="bob-x__thing" Label="@($"{A}…")" data-bob-part="thing">c</Thing>
            """;

        var info = MarkupScanner.Scan(razor);

        Assert.Equal(["root", "label", "level", "thing"], info.Parts.Select(p => p.Name).ToArray());
        Assert.Equal("div", info.Parts[0].Element);
        Assert.Equal(["bob-x"], info.Parts[0].Classes);
        Assert.Equal(["bob-x__label", "bob-visually-hidden"], info.Parts[1].Classes);
        Assert.Equal(["bob-x--"], info.Parts[2].ClassPrefixes);
        Assert.Equal("Thing", info.Parts[3].Element);
        Assert.Equal(["bob-x__thing"], info.Parts[3].Classes);
        Assert.Contains("Thing", info.ComponentTags);
    }

    [Theory]
    [InlineData("LoadingSpinner", new[] { "root", "icon", "label" })]
    [InlineData("ModalSheet", new[] { "backdrop", "sheet", "handle", "header", "title", "close", "body" })]
    [InlineData("ConfirmDialog", new[] { "root", "message", "actions", "cancel", "confirm" })]
    [InlineData("ToastHost", new[] { "host", "toast", "icon", "text" })]
    public void RealComponents_ExposeTheirDocumentedParts(string component, string[] parts)
    {
        var info = MarkupScanner.Scan(DocSources.ComponentMarkup(component));
        Assert.Equal(parts.OrderBy(p => p), info.Parts.Select(p => p.Name).OrderBy(p => p));
    }
}

public class CssReferenceTests
{
    private static CssReference Reference(string component, string css) =>
        CssReference.Build(css, MarkupScanner.Scan(DocSources.ComponentMarkup(component)), TokenSheet.Library);

    [Fact]
    public void ModalSheet_Variables_HaveDefaults_AndSayWhatTheyFeed()
    {
        var css = Reference("ModalSheet", "css/components/modal.css");

        var bg = css.Variables.Single(v => v.Name == "--bob-modal-bg");
        Assert.Equal("var(--bob-color-surface)", bg.Default);
        Assert.Contains(bg.Uses, u => u.Property == "background" && u.Target == "sheet");

        var radius = css.Variables.Single(v => v.Name == "--bob-modal-radius");
        Assert.Contains("1.25rem 1.25rem 0 0", radius.Default);
        Assert.Contains("min-width: 640px", radius.Default);
        Assert.Equal("1.25rem 1.25rem 0 0", radius.FirstFallback);
    }

    [Fact]
    public void ComponentVariables_ExcludeGlobalTokens_WhichAreListedSeparately()
    {
        var css = Reference("ModalSheet", "css/components/modal.css");
        Assert.DoesNotContain(css.Variables, v => v.Name.StartsWith("--bob-color-"));
        Assert.Contains(css.Tokens, t => t.Name == "--bob-color-surface" && t.Light == "#ffffff");
        Assert.Contains(css.Tokens, t => t.Name == "--bob-color-text" && t.Dark is not null);
    }

    [Fact]
    public void ButtonVariants_ThatSetAVariable_AreReportedAsSetBy_AndModifiersAreClassified()
    {
        var css = Reference("AsyncButton", "css/components/button.css");

        var bg = css.Variables.Single(v => v.Name == "--bob-button-bg");
        Assert.Contains(bg.SetBy, s => s.Contains(".bob-button--primary"));
        Assert.Equal(ClassKind.Modifier, css.Classes.Single(c => c.Name == "bob-button--primary").Kind);
        Assert.Equal(ClassKind.Block, css.Classes.Single(c => c.Name == "bob-button").Kind);
    }

    [Fact]
    public void Parts_ArePickedUpFromMarkup_WithTheRulesAndVariablesThatStyleThem()
    {
        var css = Reference("ModalSheet", "css/components/modal.css");

        var sheet = css.Parts.Single(p => p.Name == "sheet");
        Assert.Equal("div", sheet.Element);
        Assert.Contains("bob-modal", sheet.Classes);
        Assert.Contains("--bob-modal-max-width", sheet.Variables);

        var close = css.Parts.Single(p => p.Name == "close");
        Assert.Contains(close.Rules, r => r.State == ":hover" && r.Properties.Contains("background"));
    }

    [Fact]
    public void States_CoverModifiers_Ancestors_AndMedia()
    {
        var toast = Reference("ToastHost", "css/components/toast.css");
        var icon = toast.Rules.Where(r => r.Part == "icon" && r.State.Contains("inside")).ToList();
        Assert.Contains(icon, r => r.State.Contains(".bob-toast--success"));
        Assert.Equal(["bob-toast--error", "bob-toast--info", "bob-toast--success"],
            toast.Parts.Single(p => p.Name == "toast").Variants.OrderBy(v => v).ToArray());

        var modal = Reference("ModalSheet", "css/components/modal.css");
        Assert.Contains(modal.Rules, r => r.State.Contains("min-width: 640px"));
    }

    [Fact]
    public void UtilityClass_FromBaseCss_IsAttributedToBase()
    {
        var spinner = Reference("LoadingSpinner", "css/components/spinner.css");
        var hidden = spinner.Classes.Single(c => c.Name == "bob-visually-hidden");
        Assert.Equal(ClassKind.Utility, hidden.Kind);
        Assert.Equal("css/base.css", hidden.DefinedIn);
        Assert.Contains("bob-spin", spinner.Keyframes);
    }

    [Fact]
    public void Snippets_AreGeneratedFromTheReference()
    {
        var css = Reference("ModalSheet", "css/components/modal.css");
        var block = CssSnippets.VariableBlock("ModalSheet", css);
        Assert.Contains("--bob-modal-bg: var(--bob-color-surface);", block);
        Assert.StartsWith("/* ModalSheet", block);

        var part = css.Parts.Single(p => p.Name == "sheet");
        Assert.Equal(".bob-modal[data-bob-part=\"sheet\"]", CssSnippets.PartSelector(part));
        Assert.Contains(".bob-modal[data-bob-part=\"sheet\"] {", CssSnippets.PartBlock("ModalSheet", css));
    }
}

public class ApiReflectorTests
{
    [Fact]
    public void ModalSheet_Parameters_AreSplitByKind_WithTypesDefaultsAndXmlSummaries()
    {
        var api = ApiReflector.Describe(typeof(ModalSheet));

        Assert.Equal("ModalSheet", api.Name);
        Assert.False(string.IsNullOrWhiteSpace(api.Summary));

        var show = api.Parameters.Single(p => p.Name == "Show");
        Assert.Equal("bool", show.Type);
        Assert.Equal("false", show.Default);
        Assert.Contains("caller owns this state", show.Summary);

        Assert.Equal(ApiKind.Event, api.Parameters.Single(p => p.Name == "OnClose").Kind);
        Assert.Equal("EventCallback", api.Parameters.Single(p => p.Name == "OnClose").Type);
        Assert.Equal(ApiKind.Slot, api.Parameters.Single(p => p.Name == "HeaderContent").Kind);
        Assert.Equal("RenderFragment?", api.Parameters.Single(p => p.Name == "HeaderContent").Type);
        Assert.Equal("true", api.Parameters.Single(p => p.Name == "EnableBackButton").Default);
        Assert.Equal("string?", api.Parameters.Single(p => p.Name == "Title").Type);
        Assert.Equal("null", api.Parameters.Single(p => p.Name == "Title").Default);
    }

    [Fact]
    public void EditorRequired_IsFlaggedRequired()
    {
        var api = ApiReflector.Describe(typeof(BobsComponents.Components.BobErrorCard));
        Assert.True(api.Parameters.Single(p => p.Name == "Error").Required);
        Assert.DoesNotContain(api.Parameters.Where(p => p.Name != "Error"), p => p.Required);
    }

    [Fact]
    public void NullableEnums_CaptureUnmatched_AndStringDefaults_AreDescribed()
    {
        var api = ApiReflector.Describe(typeof(AsyncButton));
        Assert.Equal("BobButtonVariant?", api.Parameters.Single(p => p.Name == "Variant").Type);
        Assert.Equal("null", api.Parameters.Single(p => p.Name == "Variant").Default);
        Assert.Equal("\"button\"", api.Parameters.Single(p => p.Name == "Type").Default);
        Assert.Equal("16", api.Parameters.Single(p => p.Name == "SpinnerSize").Default);
        Assert.Equal(ApiKind.Attributes, api.Parameters.Single(p => p.Name == "AdditionalAttributes").Kind);
        Assert.All(api.Parameters, p => Assert.False(p.Inherited));
    }

    [Fact]
    public void Service_Members_AreListedWithSummaries()
    {
        var members = ApiReflector.Members(typeof(IBobToastService));
        var show = members.Single(m => m.Name == "Show");
        Assert.Contains("Show(string text", show.Signature);
        Assert.Contains("BobToastLevel level = BobToastLevel.Success", show.Signature);
    }

    [Fact]
    public void XmlDocs_RenderCodeAndCrefs_AsBacktickedText()
    {
        var xml = """
            <doc><members><member name="T:A.B"><summary>Use <see cref="P:A.B.Title"/> or <c>null</c> for <paramref name="x"/>,
              <see langword="true"/>.</summary></member></members></doc>
            """;
        var docs = XmlDocs.Load(xml);
        Assert.Equal("Use `Title` or `null` for `x`, `true`.", docs.Summary(typeof(A.B)));
    }
}

public class DomMapTests
{
    [Fact]
    public void ParsesRenderedMarkup_IntoLabelledRows_WithParentParts()
    {
        const string html = """
            <!--!--><div class="bob-modal-backdrop" data-bob-part="backdrop">
              <div class="bob-modal extra" data-bob-part="sheet" role="dialog" aria-modal="true">
                <svg width="1"><path d="M0"></path></svg>
                <h3 class="bob-modal__title" data-bob-part="title">Edit &amp; save</h3>
                <input id="n" autofocus>
              </div>
            </div>
            """;

        var rows = DomMap.Build(html);

        Assert.Equal(["div", "div", "svg", "h3", "input"], rows.Select(r => r.Tag).ToArray());
        Assert.Equal([0, 1, 2, 3, 4], rows.Select(r => r.Index).ToArray());
        var sheet = rows[1];
        Assert.Equal("sheet", sheet.Part);
        Assert.Equal("backdrop", sheet.ParentPart);
        Assert.Equal(["bob-modal"], sheet.BobClasses);
        Assert.Equal(["extra"], sheet.OtherClasses);
        Assert.Contains("role=dialog", sheet.Attributes);
        Assert.Contains("aria-modal=true", sheet.Attributes);
        Assert.Equal("Edit & save", rows[3].Text);
        Assert.Equal("sheet", rows[3].ParentPart);
        Assert.Equal(["sheet"], DomMap.Placement(rows, "title"));
        Assert.Equal(["(top level)"], DomMap.Placement(rows, "backdrop"));
    }

    [Fact]
    public void VoidElements_QuotedGreaterThan_AndStrayClosers_DoNotBreakTheTree()
    {
        var rows = DomMap.Build("<p>a<br>b</p></span><button title=\"a > b\" disabled>x</button><img src=x.png>");
        Assert.Equal(["p", "br", "button", "img"], rows.Select(r => r.Tag).ToArray());
        Assert.All(rows.Where(r => r.Tag != "br"), r => Assert.Equal(0, r.Depth));
        Assert.Equal(1, rows[1].Depth);
        Assert.Contains("disabled", rows[2].Attributes);
    }
}

public class DocSearchTests
{
    [Fact]
    public void Tokenize_DropsQuestionWords()
    {
        Assert.Equal(["input", "border"], DocSearch.Tokenize("What changes the input border?"));
    }

    [Fact]
    public void FindsComponents_Parts_Classes_Variables_Parameters_AndErrors()
    {
        Assert.Equal("ModalSheet", DocSearch.Search("modalsheet")[0].Entry.Title);
        Assert.Contains(DocSearch.Search("--bob-modal-bg"), h => h.Entry.Kind == SearchKind.Variable && h.Entry.Title == "--bob-modal-bg");
        Assert.Contains(DocSearch.Search("backdrop"), h => h.Entry.Kind == SearchKind.Part && h.Entry.Component == "ModalSheet");
        Assert.Contains(DocSearch.Search("bob-toast__icon"), h => h.Entry.Kind == SearchKind.Class);
        Assert.Contains(DocSearch.Search("EnableBackButton"), h => h.Entry.Kind == SearchKind.Parameter);
        Assert.Contains(DocSearch.Search("BOB-PARAM-001"), h => h.Entry.Kind == SearchKind.Error && h.Entry.Href == "bob/error-catalog#BOB-PARAM-001");
    }

    [Fact]
    public void EffectQueries_FindTheDeclarationAndTheVariableBehindIt()
    {
        var hits = DocSearch.Search("what changes the sheet border");
        var effect = hits.First(h => h.Entry.Kind == SearchKind.Effect && h.Entry.Title.StartsWith("border of sheet"));
        Assert.Contains("--bob-modal-border", effect.Entry.Detail);
        Assert.Equal("ModalSheet", effect.Entry.Component);
    }

    [Fact]
    public void WhenNoEntryMatchesEveryWord_FallsBackToAnyWord()
    {
        var hits = DocSearch.Search("zzzznotaword border");
        Assert.NotEmpty(hits);
        Assert.All(hits, h => Assert.Contains("border", h.Entry.Haystack));
    }

    [Fact]
    public void EmptyAndNoise_ReturnNothing()
    {
        Assert.Empty(DocSearch.Search("   "));
        Assert.Empty(DocSearch.Search("what is the"));
        Assert.Empty(DocSearch.Search("qqqqzzzz"));
    }

    [Fact]
    public void ResultHrefs_PointAtAnchorsThePageActuallyRenders()
    {
        // Anchors the search emits must be ids the templates produce (see DocIds).
        var variable = DocSearch.Search("--bob-modal-bg").First(h => h.Entry.Kind == SearchKind.Variable);
        Assert.EndsWith("#" + DocIds.Var("--bob-modal-bg"), variable.Entry.Href);
        var part = DocSearch.Search("backdrop").First(h => h.Entry.Kind == SearchKind.Part);
        Assert.EndsWith("#" + DocIds.Part("backdrop"), part.Entry.Href);
    }
}

public class ChangelogAndA11yParserTests
{
    [Fact]
    public void Changelog_ListsBreakingFirst_EvenWhenWrittenLast_AndFlagsReleasesWithoutBreaking()
    {
        const string md = """
            # Changelog

            ## 0.2.0 - 2026-11-01
            ### Added
            - A new `bob-x` class
            ### Fixed
            - Something
            ### Breaking
            - Renamed `--bob-a` to `--bob-b`
              (continued line)

            ## 0.1.0 - 2026-10-18
            ### Added
            - First
            """;

        var releases = ChangelogParser.Parse(md);

        Assert.Equal(["0.2.0", "0.1.0"], releases.Select(r => r.Version).ToArray());
        Assert.Equal(["Breaking", "Added", "Fixed"], releases[0].Sections.Select(s => s.Title).ToArray());
        Assert.True(releases[0].HasBreaking);
        Assert.Equal("Renamed `--bob-a` to `--bob-b` (continued line)", releases[0].Sections[0].Items.Single());
        Assert.False(releases[1].HasBreaking);
        Assert.Equal("2026-11-01", releases[0].Date);
    }

    [Fact]
    public void A11yNotes_ParseTestsAndGaps()
    {
        const string md = """
            # Thing
            Seeded from BobFit ACCESSIBILITY.md: "X".

            - Does a thing (WCAG 4.1.2). [test: ThingTests.A, ThingTests.B]
            - Wraps onto
              a second line. [tests: ThingTests.C]
            - gap: not tested yet
            """;

        var page = A11yNotesParser.Parse(md);

        Assert.Equal("Thing", page.Title);
        Assert.StartsWith("Seeded from", page.Source);
        Assert.Equal(3, page.Notes.Count);
        Assert.Equal(["ThingTests.A", "ThingTests.B"], page.Notes[0].Tests);
        Assert.Equal("Does a thing (WCAG 4.1.2).", page.Notes[0].Text);
        Assert.Equal("Wraps onto a second line.", page.Notes[1].Text);
        Assert.Equal(["ThingTests.C"], page.Notes[1].Tests);
        Assert.True(page.Notes[2].IsGap);
        Assert.Equal("not tested yet", page.Notes[2].Text);
        Assert.Empty(page.Notes[2].Tests);
    }
}
