using System.Reflection;
using Bunit;
using BobsComponents.Components;
using BobsComponents.Errors;
using BobsComponents.Options;
using Microsoft.Extensions.DependencyInjection;

namespace BobsComponents.Tests.Errors;

public class CatalogTests
{
    private static readonly BobErrorCatalog Catalog = BobErrorCatalog.CreateDefault();

    [Fact]
    public void EveryCodeTheLibraryCanRaiseIsInTheCatalog()
    {
        var codeTypes = new[]
        {
            typeof(BobParameterException), typeof(BobDataShapeException), typeof(BobMissingDependencyException),
            typeof(BobHttpException), typeof(BobCallbackException), typeof(BobErrorClassifier),
        };
        var codes = codeTypes
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(f => f.IsLiteral && f.FieldType == typeof(string) && f.Name.EndsWith("Code", StringComparison.Ordinal))
            .Select(f => (string)f.GetRawConstantValue()!)
            .Distinct()
            .ToList();

        Assert.NotEmpty(codes);
        foreach (var code in codes)
            Assert.True(Catalog.Get(code) is not null, $"Catalog is missing {code}");
    }

    [Fact]
    public void EveryOriginHasAtLeastOneEntry()
    {
        foreach (var origin in Enum.GetValues<BobErrorOrigin>())
            Assert.Contains(Catalog.All, e => e.Origin == origin);
    }

    [Fact]
    public void EntriesHaveAllFields_AndCodesUseThePrefix()
    {
        foreach (var e in Catalog.All)
        {
            Assert.StartsWith("BOB-", e.Code);
            Assert.False(string.IsNullOrWhiteSpace(e.Title));
            Assert.False(string.IsNullOrWhiteSpace(e.Message));
            Assert.False(string.IsNullOrWhiteSpace(e.Cause));
            Assert.False(string.IsNullOrWhiteSpace(e.Fix));
        }
    }

    [Fact]
    public void DuplicateCodesAreRejected_AndExtraCatalogsCanBeAdded()
    {
        var catalog = BobErrorCatalog.CreateDefault();
        Assert.Throws<InvalidOperationException>(() => catalog.AddJson(
            """[{"code":"BOB-COMP-001","origin":"ComponentBug","title":"t","message":"m","cause":"c","fix":"f"}]"""));
        catalog.AddJson("""[{"code":"APP-001","origin":"CallbackThrew","title":"t","message":"m","cause":"c","fix":"f"}]""");
        Assert.NotNull(catalog.Get("APP-001"));
    }
}

public class ErrorCardOptionTests : TestContext
{
    private static BobError SampleError()
    {
        Exception ex;
        try
        {
            try { throw new InvalidOperationException("inner detail"); }
            catch (Exception inner) { throw new InvalidOperationException("outer message", inner); }
        }
        catch (Exception e) { ex = e; }
        return new BobErrorClassifier().Classify(ex, "Sample");
    }

    private IRenderedComponent<BobErrorCard> Render(Action<BobOptions>? configure = null)
    {
        Services.AddBobComponents(configure);
        return RenderComponent<BobErrorCard>(p => p.Add(c => c.Error, SampleError()));
    }

    [Fact]
    public void Default_MessageVisible_DetailsCollapsedBehindToggle()
    {
        var cut = Render();
        Assert.Equal("outer message", cut.Find("[data-bob-part='message']").TextContent);
        var toggle = cut.Find("[data-bob-part='toggle']");
        Assert.Equal("false", toggle.GetAttribute("aria-expanded"));
        Assert.NotNull(cut.Find("[data-bob-part='details']").GetAttribute("hidden"));

        toggle.Click();

        Assert.Equal("true", cut.Find("[data-bob-part='toggle']").GetAttribute("aria-expanded"));
        Assert.Null(cut.Find("[data-bob-part='details']").GetAttribute("hidden"));
        Assert.Contains("inner detail", cut.Find("[data-bob-part='inner']").TextContent);
        Assert.NotEmpty(cut.Find("[data-bob-part='stack']").TextContent);
    }

    [Fact]
    public void AlwaysOpen_HasNoToggle_AndDetailsShown()
    {
        var cut = Render(o => o.Errors.Details = BobErrorDetailsMode.AlwaysOpen);
        Assert.Empty(cut.FindAll("[data-bob-part='toggle']"));
        Assert.Null(cut.Find("[data-bob-part='details']").GetAttribute("hidden"));
    }

    [Fact]
    public void Hidden_StillShowsExactMessage_ButNoDetails()
    {
        var cut = Render(o => o.Errors.Details = BobErrorDetailsMode.Hidden);
        Assert.Equal("outer message", cut.Find("[data-bob-part='message']").TextContent);
        Assert.Empty(cut.FindAll("[data-bob-part='details']"));
        Assert.Empty(cut.FindAll("[data-bob-part='toggle']"));
    }

    [Fact]
    public void StackAndInnerCanBeTurnedOffIndependently()
    {
        var cut = Render(o => { o.Errors.ShowStackTrace = false; o.Errors.ShowInnerExceptions = true; o.Errors.Details = BobErrorDetailsMode.AlwaysOpen; });
        Assert.Empty(cut.FindAll("[data-bob-part='stack']"));
        Assert.NotEmpty(cut.FindAll("[data-bob-part='inner']"));
    }

    [Fact]
    public void ErrorId_AndCatalogLink_AreConfigurable()
    {
        var cut = Render(o => o.Errors.CatalogBaseUrl = "/docs/errors");
        Assert.Equal("/docs/errors#BOB-CB-001", cut.Find("a[data-bob-part='code']").GetAttribute("href"));
        Assert.NotEmpty(cut.FindAll("[data-bob-part='id']"));
    }

    [Fact]
    public void ErrorId_CanBeHidden()
    {
        var cut = Render(o => o.Errors.ShowErrorId = false);
        Assert.Empty(cut.FindAll("[data-bob-part='id']"));
    }
}
