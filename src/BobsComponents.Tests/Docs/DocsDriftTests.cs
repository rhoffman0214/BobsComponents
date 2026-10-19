using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Bunit;
using BobsComponents.Components;
using BobsComponents.Docs.Generation;
using BobsComponents.Errors;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BobsComponents.Tests.Docs;

/// <summary>
/// The docs pages are generated, so these tests check the two things that can still drift: that the generators see
/// everything in the source (checked here against the raw files, independently of the generators), and that nothing
/// in the library is missing from the docs registry.
/// </summary>
public class DocsDriftTests
{
    private static readonly string Root = FindRoot();

    internal static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "docs", "CSS-CONTRACT.md"))) dir = dir.Parent;
        return dir!.FullName;
    }

    private static string Lib(params string[] parts) => Path.Combine([Root, "src", "BobsComponents", .. parts]);

    public static IEnumerable<object[]> Docs() => DocRegistry.Components.Select(c => new object[] { c.Slug });

    private static readonly HashSet<string> DocumentedElsewhere = ["BobErrorCard", "BobDevKit"];

    [Fact]
    public void Registry_CoversEveryLibraryComponent()
    {
        var components = typeof(LoadingSpinner).Assembly.GetTypes()
            .Where(t => t.Namespace == "BobsComponents.Components" && typeof(ComponentBase).IsAssignableFrom(t) && !t.IsAbstract && t.IsPublic)
            .Select(t => t.Name)
            .ToHashSet();

        var documented = DocRegistry.Components.Select(c => c.Component.Name).Concat(DocRegistry.ReferenceSheets.Select(s => s.MarkupComponent)).ToHashSet();

        Assert.Empty(components.Except(documented));
        Assert.Empty(documented.Except(components));
        Assert.All(DocumentedElsewhere, name => Assert.Contains(name, documented));
    }

    [Fact]
    public void Registry_ClaimsEveryComponentStylesheet_ExactlyOnce()
    {
        var onDisk = Directory.GetFiles(Lib("wwwroot", "css", "components"), "*.css").Select(Path.GetFileName).OrderBy(n => n).ToArray();
        var claimed = DocRegistry.Components.Select(c => c.CssFile).Concat(DocRegistry.ReferenceSheets.Select(s => s.CssFile))
            .Select(Path.GetFileName).OrderBy(n => n).ToArray();
        Assert.Equal(onDisk, claimed);
    }

    [Fact]
    public void Registry_HasUniqueSlugs_AndEveryComponentHasADemo()
    {
        Assert.Equal(DocRegistry.Components.Count, DocRegistry.Components.Select(c => c.Slug).Distinct().Count());
        Assert.All(DocRegistry.Components, c => Assert.NotEmpty(c.Demos));
        Assert.All(DocRegistry.Components.SelectMany(c => c.Demos), d => Assert.True(typeof(ComponentBase).IsAssignableFrom(d.Type)));
    }

    [Theory]
    [MemberData(nameof(Docs))]
    public void Component_HasASummary_AndEveryOwnParameterHasAnXmlDocComment(string slug)
    {
        var doc = DocRegistry.Find(slug)!;
        var model = DocRegistry.Model(doc);

        Assert.False(string.IsNullOrWhiteSpace(doc.Component.GetCustomAttribute<DescriptionAttribute>()?.Description),
            $"{doc.Component.Name} needs @attribute [System.ComponentModel.Description(\"...\")]");
        foreach (var p in model.Api.Parameters.Where(p => !p.Inherited))
            Assert.False(string.IsNullOrWhiteSpace(p.Summary), $"{doc.Component.Name}.{p.Name} needs a /// <summary> comment");
    }

    [Theory]
    [MemberData(nameof(Docs))]
    public void GeneratedModel_ListsEveryParameter_Part_AndVariable_FoundInTheRawSource(string slug)
    {
        var doc = DocRegistry.Find(slug)!;
        var model = DocRegistry.Model(doc);

        var parameters = doc.Component.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<ParameterAttribute>() is not null || p.GetCustomAttribute<CascadingParameterAttribute>() is not null)
            .Select(p => p.Name);
        foreach (var name in parameters)
            Assert.Contains(model.Api.Parameters, p => p.Name == name);

        var markup = File.ReadAllText(Lib("Components", doc.Component.Name + ".razor"));
        foreach (var part in Regex.Matches(markup, @"data-bob-part=""([\w-]+)""").Select(m => m.Groups[1].Value).Distinct())
            Assert.Contains(model.Css.Parts, p => p.Name == part);

        var sheet = File.ReadAllText(Lib("wwwroot", doc.CssFile.Replace('/', Path.DirectorySeparatorChar)));
        var tokens = TokenSheet.Library;
        foreach (var variable in Regex.Matches(sheet, @"--bob-[\w-]+").Select(m => m.Value).Distinct().Where(v => !tokens.IsToken(v)))
            Assert.Contains(model.Css.Variables, v => v.Name == variable);

        foreach (var cls in Regex.Matches(sheet, @"\.(bob-[\w-]+)").Select(m => m.Groups[1].Value).Distinct())
            Assert.Contains(model.Css.Classes, c => c.Name == cls);
    }

    [Theory]
    [MemberData(nameof(Docs))]
    public void HookClasses_TheMarkupWritesWithoutARule_AreDocumentedInTheContract(string slug)
    {
        // A class with no rule anywhere is a deliberate styling hook for the app's own CSS. It is public API, so the contract must list it.
        var contract = File.ReadAllText(Path.Combine(Root, "docs", "CSS-CONTRACT.md"));
        var model = DocRegistry.Model(DocRegistry.Find(slug)!);
        var missing = model.Css.Classes.Where(c => c.DefinedIn == CssReference.HookOnly && !contract.Contains(c.Name)).Select(c => c.Name).ToList();
        Assert.True(missing.Count == 0, $"{slug}: hook classes missing from docs/CSS-CONTRACT.md: {string.Join(", ", missing)}");
    }

    [Theory]
    [MemberData(nameof(Docs))]
    public void Demos_AreEmbeddedAsTheirOwnSource(string slug)
    {
        var doc = DocRegistry.Find(slug)!;
        foreach (var demo in doc.Demos)
        {
            var onDisk = Path.Combine(Root, "src", "BobsComponents.Docs", "Demos", demo.SourceName + ".razor");
            Assert.True(File.Exists(onDisk), onDisk);
            Assert.Equal(File.ReadAllText(onDisk), DocSources.DemoSource(demo.SourceName));
        }
    }

    [Theory]
    [MemberData(nameof(Docs))]
    public void Accessibility_Exists_AndEveryNoteNamesATestThatExists(string slug)
    {
        var model = DocRegistry.Model(DocRegistry.Find(slug)!);
        Assert.NotNull(model.Accessibility);
        Assert.NotEmpty(model.Accessibility!.Notes);

        foreach (var note in model.Accessibility.Notes.Where(n => !n.IsGap))
        {
            Assert.True(note.Tests.Count > 0, $"{slug}: no [test: ...] on \"{note.Text}\"");
            foreach (var test in note.Tests) Assert.True(TestExists(test), $"{slug}: test '{test}' does not exist");
        }
    }

    private static bool TestExists(string name)
    {
        var dot = name.LastIndexOf('.');
        if (dot < 0) return false;
        var type = typeof(DocsDriftTests).Assembly.GetTypes().FirstOrDefault(t => t.Name == name[..dot]);
        var method = type?.GetMethod(name[(dot + 1)..], BindingFlags.Public | BindingFlags.Instance);
        return method is not null && (method.GetCustomAttribute<FactAttribute>() is not null || method.GetCustomAttribute<TheoryAttribute>() is not null);
    }

    [Fact]
    public void ErrorUses_AreDerivedFromTheSource_AndExistInTheCatalog()
    {
        var modal = DocRegistry.Model(DocRegistry.Find("modal-sheet")!);
        Assert.Equal(["BOB-ASSET-001", "BOB-CB-001", "BOB-PARAM-002"], modal.Errors.Select(e => e.Entry.Code).ToArray());
        Assert.Contains("focusTrap.js", modal.Errors.First(e => e.Entry.Code == "BOB-ASSET-001").Because);

        var toast = DocRegistry.Model(DocRegistry.Find("toasts")!);
        Assert.Contains(toast.Errors, e => e.Entry.Code == "BOB-ASSET-002" && e.Because.Contains("IBobToastService"));

        var confirm = DocRegistry.Model(DocRegistry.Find("confirm-dialog")!);
        Assert.Contains(confirm.Errors, e => e.Entry.Code == "BOB-ASSET-001" && e.Via == "ModalSheet");
        Assert.Contains(confirm.Errors, e => e.Entry.Code == "BOB-PARAM-001" && e.Via is null);

        foreach (var doc in DocRegistry.Components)
            Assert.All(DocRegistry.Model(doc).Errors, e => Assert.NotNull(DocRegistry.Catalog.Get(e.Entry.Code)));
    }
}

/// <summary>Renders every demo for real and checks the markup it produces against the generated CSS reference.</summary>
public class DemoRenderTests : TestContext
{
    public DemoRenderTests()
    {
        Services.AddBobComponents();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    public static IEnumerable<object[]> Demos() =>
        DocRegistry.Components.SelectMany(c => c.Demos.Select(d => new object[] { c.Slug, d.SourceName }));

    [Theory]
    [MemberData(nameof(Demos))]
    public void Demo_Renders_WithOnlyClassesAndPartsTheReferenceKnows(string slug, string demoName)
    {
        var doc = DocRegistry.Find(slug)!;
        var demo = doc.Demos.First(d => d.SourceName == demoName);

        var cut = RenderComponent(typeof(DynamicComponent), demo.Type);
        AssertKnown(doc, cut.Markup);

        // Open whatever the demo hides behind its first buttons (sheets, dialogs, toasts) and check again.
        var buttonCount = Math.Min(3, cut.FindAll("button").Count);
        for (var i = 0; i < buttonCount; i++)
        {
            cut.FindAll("button")[i].Click();
            AssertKnown(doc, cut.Markup);
        }
    }

    private static void AssertKnown(ComponentDoc doc, string html)
    {
        var models = new[] { DocRegistry.Model(doc) }.Concat(DocRegistry.Model(doc).Composes.Select(DocRegistry.Model)).ToList();
        var knownParts = models.SelectMany(m => m.Css.Parts.Select(p => p.Name)).ToHashSet();
        var knownClasses = models.SelectMany(m => m.Css.Classes.Select(c => c.Name)).ToHashSet();

        foreach (var row in DomMap.Build(html))
        {
            if (row.Part is not null) Assert.True(knownParts.Contains(row.Part), $"{doc.Slug}: rendered part '{row.Part}' is not in the generated reference");
            foreach (var cls in row.BobClasses)
                Assert.True(knownClasses.Contains(cls) || KnownGlobal(cls) || models.Any(m => m.Css.DynamicClassPatterns.Any(cls.StartsWith)), $"{doc.Slug}: rendered class '{cls}' is not in the generated reference");
        }
    }

    private static bool KnownGlobal(string cls) =>
        DocRegistry.Components.Any(c => DocRegistry.Model(c).Css.Classes.Any(k => k.Name == cls));

    [Fact]
    public void ModalSheetDemo_ShowsEveryDocumentedPart_OnceOpened()
    {
        var doc = DocRegistry.Find("modal-sheet")!;
        var cut = RenderComponent(typeof(DynamicComponent), doc.Demos[0].Type);
        cut.Find("button.bob-button--primary").Click();

        var rendered = DomMap.Build(cut.Markup).Select(r => r.Part).Where(p => p is not null).ToHashSet();
        var documented = DocRegistry.Model(doc).Css.Parts.Select(p => p.Name).ToHashSet();
        Assert.Empty(documented.Except(rendered));
    }

    private IRenderedFragment RenderComponent(Type host, Type demoType) =>
        RenderComponent<DynamicComponent>(p => p.Add(c => c.Type, demoType));
}
