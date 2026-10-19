using Bunit;
using BobsComponents.Components;
using BobsComponents.Infrastructure;

namespace BobsComponents.Tests.Components;

public class RestPickerTests : TestContext
{
    public RestPickerTests() => Services.AddBobComponents();

    [Fact]
    public void IsAFieldsetWithALegend()
    {
        var cut = RenderComponent<RestPicker>();
        Assert.Equal("fieldset", cut.Find("[data-bob-part='root']").TagName.ToLowerInvariant());
        Assert.Equal("Rest between sets", cut.Find("legend").TextContent);
    }

    [Fact]
    public void NoValue_ShowsTheDefaultAsPressed()
    {
        var cut = RenderComponent<RestPicker>();
        var pressed = cut.FindAll("[data-bob-part='chip'][aria-pressed='true']");
        Assert.Single(pressed);
        Assert.Equal("90s", pressed[0].TextContent);
        Assert.Equal("90s between sets.", cut.Find("[data-bob-part='hint']").TextContent);
    }

    [Fact]
    public void ChipsExposeTheirPressedState()
    {
        var cut = RenderComponent<RestPicker>(p => p.Add(c => c.Value, 45));
        var chips = cut.FindAll("[data-bob-part='chip']");
        Assert.Equal("true", chips.Single(b => b.TextContent == "45s").GetAttribute("aria-pressed"));
        Assert.Equal("false", chips.Single(b => b.TextContent == "60s").GetAttribute("aria-pressed"));
    }

    [Fact]
    public void ClickingAChip_RaisesValueChanged()
    {
        int? got = null;
        var cut = RenderComponent<RestPicker>(p => p.Add(c => c.ValueChanged, (int? v) => got = v));
        cut.FindAll("[data-bob-part='chip']").Single(b => b.TextContent == "120s").Click();
        Assert.Equal(120, got);
    }

    [Fact]
    public void OtherBox_ClampsToRange_AndClearsOnJunk()
    {
        int? got = -1;
        var cut = RenderComponent<RestPicker>(p => p.Add(c => c.ValueChanged, (int? v) => got = v));
        cut.Find("[data-bob-part='input']").Change("5");
        Assert.Equal(15, got);
        cut.Find("[data-bob-part='input']").Change("99999");
        Assert.Equal(1800, got);
        cut.Find("[data-bob-part='input']").Change("abc");
        Assert.Null(got);
    }

    [Fact]
    public void OtherBox_ShowsOnlyCustomValues()
    {
        var preset = RenderComponent<RestPicker>(p => p.Add(c => c.Value, 60));
        Assert.True(string.IsNullOrEmpty(preset.Find("[data-bob-part='input']").GetAttribute("value")));
        var custom = RenderComponent<RestPicker>(p => p.Add(c => c.Value, 200));
        Assert.Equal("200", custom.Find("[data-bob-part='input']").GetAttribute("value"));
    }

    [Fact]
    public void OtherBoxIsLabelled()
    {
        var cut = RenderComponent<RestPicker>();
        var label = cut.Find("[data-bob-part='other']");
        Assert.Equal("label", label.TagName.ToLowerInvariant());
        Assert.Contains("Other (s)", label.TextContent);
        Assert.NotNull(label.QuerySelector("input"));
    }

    [Fact]
    public void Disabled_DisablesTheGroup()
    {
        var cut = RenderComponent<RestPicker>(p => p.Add(c => c.Disabled, true));
        Assert.True(cut.Find("[data-bob-part='root']").HasAttribute("disabled"));
    }

    [Fact]
    public void ChipsAreKeyboardOperableButtons()
    {
        var cut = RenderComponent<RestPicker>();
        Assert.All(cut.FindAll("[data-bob-part='chip']"), b =>
        {
            Assert.Equal("button", b.TagName.ToLowerInvariant());
            Assert.Equal("button", b.GetAttribute("type"));
        });
    }
}
