using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using BobsComponents.Components;
using BobsComponents.Errors;
using BobsComponents.Infrastructure;
using BobsComponents.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BobsComponents.Tests.Contract;

/// <summary>Guards the naming contract documented in docs/CSS-CONTRACT.md.</summary>
public partial class ContractTests : TestContext
{
    private static readonly string Root = FindRoot();
    private static readonly string Wwwroot = Path.Combine(Root, "src", "BobsComponents", "wwwroot");

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "docs", "CSS-CONTRACT.md")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Could not find the repo root (docs/CSS-CONTRACT.md).");
    }

    private static IEnumerable<string> CssFiles() =>
        Directory.EnumerateFiles(Wwwroot, "*.css", SearchOption.AllDirectories);

    private static string StripComments(string css) => CommentRegex().Replace(css, "");

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CommentRegex();

    [GeneratedRegex(@"(?<![\w-])\.(?<name>[A-Za-z_][\w-]*)")]
    private static partial Regex ClassRegex();

    [GeneratedRegex(@"(?<![\w-])--[\w-]+")]
    private static partial Regex CustomPropertyRegex();

    [GeneratedRegex(@"\[(?<name>data-[\w-]+)")]
    private static partial Regex DataAttrRegex();

    /// <summary>Selector text: everything before each '{' that is not an at-rule.</summary>
    private static IEnumerable<string> Selectors(string css)
    {
        var text = StripComments(css);
        foreach (Match m in Regex.Matches(text, @"(?<sel>[^{};]+)\{"))
        {
            var sel = m.Groups["sel"].Value.Trim();
            if (!sel.StartsWith('@')) yield return sel;
        }
    }

    [Fact]
    public void EveryCssClass_StartsWithBob()
    {
        foreach (var file in CssFiles())
            foreach (var selector in Selectors(File.ReadAllText(file)))
            {
                var withoutStrings = Regex.Replace(selector, "\"[^\"]*\"", "\"\"");
                foreach (Match m in ClassRegex().Matches(withoutStrings))
                    Assert.True(m.Groups["name"].Value.StartsWith("bob-", StringComparison.Ordinal),
                        $"{Path.GetFileName(file)}: class '.{m.Groups["name"].Value}' must start with 'bob-'");
            }
    }

    [Fact]
    public void EveryCustomProperty_StartsWithBob()
    {
        foreach (var file in CssFiles())
            foreach (Match m in CustomPropertyRegex().Matches(StripComments(File.ReadAllText(file))))
                Assert.True(m.Value.StartsWith("--bob-", StringComparison.Ordinal),
                    $"{Path.GetFileName(file)}: custom property '{m.Value}' must start with '--bob-'");
    }

    [Fact]
    public void EveryDataAttributeSelector_StartsWithDataBob()
    {
        foreach (var file in CssFiles())
            foreach (Match m in DataAttrRegex().Matches(StripComments(File.ReadAllText(file))))
                Assert.StartsWith("data-bob-", m.Groups["name"].Value);
    }

    [Fact]
    public void EveryCustomPropertyUsed_IsDocumented()
    {
        var doc = File.ReadAllText(Path.Combine(Root, "docs", "CSS-CONTRACT.md"));
        var used = CssFiles()
            .SelectMany(f => CustomPropertyRegex().Matches(StripComments(File.ReadAllText(f))).Select(m => m.Value))
            .Distinct();
        foreach (var prop in used)
            Assert.True(doc.Contains($"`{prop}`", StringComparison.Ordinal), $"{prop} is used in CSS but missing from docs/CSS-CONTRACT.md");
    }

    [Fact]
    public void EveryClassBlock_IsDocumented()
    {
        var doc = File.ReadAllText(Path.Combine(Root, "docs", "CSS-CONTRACT.md"));
        var blocks = CssFiles()
            .SelectMany(f => Selectors(File.ReadAllText(f)))
            .SelectMany(s => ClassRegex().Matches(Regex.Replace(s, "\"[^\"]*\"", "\"\"")).Select(m => m.Groups["name"].Value))
            .Select(n => Regex.Split(n, "__|--")[0])
            .Distinct();
        foreach (var block in blocks)
            Assert.True(doc.Contains($"`{block}", StringComparison.Ordinal), $"class '{block}' is not documented");
    }

    [Fact]
    public void BobCss_DeclaresLayers_AndImportsEverySheetIntoALayer()
    {
        var entry = StripComments(File.ReadAllText(Path.Combine(Wwwroot, "bob.css"))).Trim();
        Assert.StartsWith("@layer bob.tokens, bob.base, bob.components;", entry);

        var imports = Regex.Matches(entry, @"@import url\(""(?<path>[^""]+)""\)\s*(?<rest>[^;]*);").ToList();
        Assert.NotEmpty(imports);
        foreach (var import in imports)
        {
            Assert.Matches(@"^layer\(bob\.(tokens|base|components)\)$", import.Groups["rest"].Value.Trim());
            Assert.True(File.Exists(Path.Combine(Wwwroot, import.Groups["path"].Value)), $"{import.Groups["path"].Value} does not exist");
        }

        var imported = imports.Select(i => Path.GetFullPath(Path.Combine(Wwwroot, i.Groups["path"].Value))).ToHashSet();
        foreach (var file in CssFiles().Where(f => Path.GetFileName(f) != "bob.css"))
            Assert.Contains(Path.GetFullPath(file), imported);

        // Nothing outside an imported layer: the component sheets contain no layer-less top-level rules by construction,
        // and bob.css itself holds only @layer/@import statements.
        var withoutStatements = Regex.Replace(entry, @"@(layer|import)[^;]*;", "").Trim();
        Assert.Equal("", withoutStatements);
    }

    [Fact]
    public void LibraryUsesNoScopedCss_AndNoGrid_AndNoBootstrapRequirement()
    {
        var libDir = Path.Combine(Root, "src", "BobsComponents");
        var scoped = Directory.EnumerateFiles(libDir, "*.razor.css", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")
                        && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
            .ToList();
        Assert.True(scoped.Count == 0, $"Scoped CSS is not allowed in the library: {string.Join(", ", scoped)}");
        foreach (var file in CssFiles())
        {
            var css = File.ReadAllText(file);
            Assert.DoesNotContain("display: grid", css);
            Assert.DoesNotContain("bootstrap", css, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void StaticAssetPaths_PointAtRealFiles()
    {
        Assert.Equal("./_content/BobsComponents/js/focusTrap.js", BobJs.FocusTrap);
        Assert.Equal("./_content/BobsComponents/js/modalHistory.js", BobJs.ModalHistory);
        foreach (var path in new[] { BobJs.FocusTrap, BobJs.ModalHistory })
        {
            var file = Path.Combine(Wwwroot, path["./_content/BobsComponents/".Length..].Replace('/', Path.DirectorySeparatorChar));
            Assert.True(File.Exists(file), $"{path} has no file at {file}");
        }
    }

    // ---- Rendered markup ----

    private void AssertMarkupFollowsContract(IElement root)
    {
        foreach (var el in root.QuerySelectorAll("*").Prepend(root))
        {
            foreach (var cls in el.ClassList)
                Assert.True(cls.StartsWith("bob-", StringComparison.Ordinal), $"<{el.LocalName}> has class '{cls}' without the bob- prefix");
            foreach (var attr in el.Attributes.Where(a => a.Name.StartsWith("data-", StringComparison.Ordinal)))
                Assert.True(attr.Name.StartsWith("data-bob-", StringComparison.Ordinal), $"<{el.LocalName}> has attribute '{attr.Name}' without the data-bob- prefix");
            if (el.HasAttribute("data-bob-part"))
                Assert.False(string.IsNullOrWhiteSpace(el.GetAttribute("data-bob-part")));
        }
    }

    [Fact]
    public void RenderedMarkup_FollowsThePrefixContract_ForEveryComponent()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddBobComponents(o => { o.DevKit.Enabled = true; o.DevKit.StartOpen = true; });

        var toasts = Services.GetRequiredService<IBobToastService>();
        var host = RenderComponent<ToastHost>();
        toasts.Show("saved", BobToastLevel.Success);
        toasts.Show("oops", BobToastLevel.Error);
        host.WaitForState(() => host.FindAll(".bob-toast").Count == 2);

        var components = new IRenderedFragment[]
        {
            host,
            RenderComponent<LoadingSpinner>(),
            RenderComponent<FullScreenLoader>(),
            RenderComponent<AsyncButton>(p => p.Add(c => c.Variant, BobButtonVariant.Primary).Add(c => c.Busy, true)),
            RenderComponent<ModalSheet>(p => p.Add(c => c.Show, true).Add(c => c.Title, "T").Add(c => c.ShowHandle, true)),
            RenderComponent<ConfirmDialog>(p => p.Add(c => c.Show, true)),
            RenderComponent<BobErrorCard>(p => p.Add(c => c.Error, new BobErrorClassifier().Classify(new HttpRequestException("x")))),
            RenderComponent<BobDevKit>(),
        };

        foreach (var component in components)
            foreach (var node in component.Nodes.OfType<IElement>())
                AssertMarkupFollowsContract(node);
    }
}
