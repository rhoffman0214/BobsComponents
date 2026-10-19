using Bunit;
using BobsComponents.Components;
using BobsComponents.Errors;
using BobsComponents.Infrastructure;

namespace BobsComponents.Tests.Components;

public class ProgressBarTests : TestContext
{
    public ProgressBarTests() => Services.AddBobComponents();

    [Fact]
    public void IsAProgressbar_WithNameAndValue()
    {
        var cut = RenderComponent<ProgressBar>(p => p.Add(c => c.Label, "Upload").Add(c => c.Value, 42));
        var root = cut.Find("[data-bob-part='root']");
        Assert.Equal("progressbar", root.GetAttribute("role"));
        Assert.Equal("Upload", root.GetAttribute("aria-label"));
        Assert.Equal("0", root.GetAttribute("aria-valuemin"));
        Assert.Equal("100", root.GetAttribute("aria-valuemax"));
        Assert.Equal("42", root.GetAttribute("aria-valuenow"));
        Assert.Contains("width: 42%", cut.Find("[data-bob-part='fill']").GetAttribute("style"));
    }

    [Fact]
    public void MissingLabel_IsAParameterError()
    {
        var ex = Assert.Throws<BobParameterException>(() => RenderComponent<ProgressBar>(p => p.Add(c => c.Value, 10)));
        Assert.Equal("BOB-PARAM-001", ex.Code);
    }

    [Fact]
    public void PercentText_IsHiddenFromAssistiveTech()
    {
        var cut = RenderComponent<ProgressBar>(p => p.Add(c => c.Label, "Upload").Add(c => c.Value, 7));
        var text = cut.Find("[data-bob-part='value']");
        Assert.Equal("7%", text.TextContent);
        Assert.Equal("true", text.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void NoValue_IsIndeterminate_WithoutValueNow()
    {
        var cut = RenderComponent<ProgressBar>(p => p.Add(c => c.Label, "Waiting"));
        var root = cut.Find("[data-bob-part='root']");
        Assert.False(root.HasAttribute("aria-valuenow"));
        Assert.Contains("bob-progress--indeterminate", root.ClassName);
        Assert.Empty(cut.FindAll("[data-bob-part='value']"));
    }

    [Theory]
    [InlineData(-20, "0")]
    [InlineData(250, "100")]
    public void Value_IsClamped(double given, string expected)
    {
        var cut = RenderComponent<ProgressBar>(p => p.Add(c => c.Label, "x").Add(c => c.Value, given));
        Assert.Equal(expected, cut.Find("[data-bob-part='root']").GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void Variant_AddsItsClass()
    {
        var cut = RenderComponent<ProgressBar>(p => p.Add(c => c.Label, "x").Add(c => c.Value, 1).Add(c => c.Variant, BobProgressVariant.Danger));
        Assert.Contains("bob-progress--danger", cut.Find("[data-bob-part='root']").ClassName);
    }

    [Fact]
    public void ReducedMotionRuleExists() => Assert.Contains("prefers-reduced-motion", File.ReadAllText(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "BobsComponents", "wwwroot", "css", "base.css")));
}

public class SkeletonLoaderTests : TestContext
{
    public SkeletonLoaderTests() => Services.AddBobComponents();

    [Fact]
    public void IsOneBusyStatusGroup()
    {
        var cut = RenderComponent<SkeletonLoader>(p => p.Add(c => c.Count, 3));
        var root = cut.Find("[data-bob-part='root']");
        Assert.Equal("status", root.GetAttribute("role"));
        Assert.Equal("true", root.GetAttribute("aria-busy"));
        Assert.Equal(3, cut.FindAll("[data-bob-part='shape']").Count);
    }

    [Fact]
    public void ShapesAreHidden_AndOnlyTheLabelIsRead()
    {
        var cut = RenderComponent<SkeletonLoader>(p => p.Add(c => c.Count, 2).Add(c => c.Label, "Loading profile"));
        Assert.All(cut.FindAll("[data-bob-part='shape']"), s => Assert.Equal("true", s.GetAttribute("aria-hidden")));
        var label = cut.Find("[data-bob-part='label']");
        Assert.Equal("Loading profile", label.TextContent);
        Assert.Contains("bob-visually-hidden", label.ClassName);
    }

    [Fact]
    public void SizeValues_AreSanitized()
    {
        var cut = RenderComponent<SkeletonLoader>(p => p
            .Add(c => c.Width, "50%; background: url(x)")
            .Add(c => c.Height, "2rem"));
        var style = cut.Find("[data-bob-part='shape']").GetAttribute("style")!;
        Assert.Contains("--bob-skeleton-width: 50%backgroundurlx;", style);
        Assert.DoesNotContain("url(", style);
        Assert.Contains("--bob-skeleton-height: 2rem;", style);
    }

    [Fact]
    public void Shape_AddsItsClass_AndRadiusAppliesToRectanglesOnly()
    {
        var circle = RenderComponent<SkeletonLoader>(p => p.Add(c => c.Shape, BobSkeletonShape.Circle).Add(c => c.Radius, "1rem"));
        Assert.Contains("bob-skeleton--circle", circle.Find("[data-bob-part='shape']").ClassName);
        Assert.DoesNotContain("--bob-skeleton-radius", circle.Find("[data-bob-part='shape']").GetAttribute("style"));

        var rect = RenderComponent<SkeletonLoader>(p => p.Add(c => c.Radius, "1rem"));
        Assert.Contains("--bob-skeleton-radius: 1rem;", rect.Find("[data-bob-part='shape']").GetAttribute("style"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void CountOutOfRange_IsAParameterError(int count)
    {
        var ex = Assert.Throws<BobParameterException>(() => RenderComponent<SkeletonLoader>(p => p.Add(c => c.Count, count)));
        Assert.Equal("BOB-PARAM-002", ex.Code);
    }

    [Fact]
    public void ReducedMotionRuleExists() => Assert.Contains("prefers-reduced-motion", File.ReadAllText(Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "BobsComponents", "wwwroot", "css", "base.css")));
}

public class SelectFieldTests : TestContext
{
    private static readonly IReadOnlyList<SelectField.SelectOption> Sizes =
        [new("s", "Small"), new("m", "Medium"), new("x", "Sold out", Disabled: true)];

    public SelectFieldTests() => Services.AddBobComponents();

    [Fact]
    public void LabelWrapsANativeSelect()
    {
        var cut = RenderComponent<SelectField>(p => p.Add(c => c.Label, "Size").Add(c => c.Options, Sizes));
        var root = cut.Find("[data-bob-part='root']");
        Assert.Equal("label", root.TagName.ToLowerInvariant());
        Assert.Equal("Size", cut.Find("[data-bob-part='label']").TextContent);
        Assert.Equal("select", cut.Find("[data-bob-part='control']").TagName.ToLowerInvariant());
        Assert.NotNull(root.QuerySelector("select"));
    }

    [Fact]
    public void RendersPlaceholder_ThenOptions_AndMarksTheChosenOne()
    {
        var cut = RenderComponent<SelectField>(p => p
            .Add(c => c.Label, "Size").Add(c => c.Options, Sizes).Add(c => c.Placeholder, "Choose").Add(c => c.Value, "m"));
        var options = cut.FindAll("option");
        Assert.Equal(["Choose", "Small", "Medium", "Sold out"], options.Select(o => o.TextContent));
        Assert.True(options.Single(o => o.TextContent == "Medium").HasAttribute("selected"));
        Assert.False(options.Single(o => o.TextContent == "Choose").HasAttribute("selected"));
        Assert.True(options.Single(o => o.TextContent == "Sold out").HasAttribute("disabled"));
    }

    [Fact]
    public void ChoosingAnOption_RaisesValueChanged()
    {
        string? got = "unset";
        var cut = RenderComponent<SelectField>(p => p
            .Add(c => c.Label, "Size").Add(c => c.Options, Sizes).Add(c => c.Placeholder, "Choose")
            .Add(c => c.ValueChanged, (string? v) => got = v));
        cut.Find("select").Change("s");
        Assert.Equal("s", got);
        cut.Find("select").Change("");
        Assert.Null(got);
    }

    [Fact]
    public void RequiredAndDisabled_ReachTheSelect()
    {
        var cut = RenderComponent<SelectField>(p => p
            .Add(c => c.Label, "Size").Add(c => c.Options, Sizes).Add(c => c.Required, true).Add(c => c.Disabled, true));
        var select = cut.Find("select");
        Assert.True(select.HasAttribute("required"));
        Assert.True(select.HasAttribute("disabled"));
    }

    [Fact]
    public void MissingLabel_IsAParameterError()
    {
        var ex = Assert.Throws<BobParameterException>(() => RenderComponent<SelectField>(p => p.Add(c => c.Options, Sizes)));
        Assert.Equal("BOB-PARAM-001", ex.Code);
    }

    [Fact]
    public void DuplicateValues_AreAParameterError()
    {
        IReadOnlyList<SelectField.SelectOption> dup = [new("a", "One"), new("a", "Two")];
        Assert.Throws<BobParameterException>(() => RenderComponent<SelectField>(p => p.Add(c => c.Label, "x").Add(c => c.Options, dup)));
    }
}
