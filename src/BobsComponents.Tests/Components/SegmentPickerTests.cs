using Bunit;
using BobsComponents.Components;
using BobsComponents.Infrastructure;
using Microsoft.AspNetCore.Components.Web;

namespace BobsComponents.Tests.Components;

public class SegmentPickerTests : TestContext
{
    private static readonly string[] Presets = ["Back", "Quads", "Calves"];

    public SegmentPickerTests() => Services.AddBobComponents();

    private IRenderedComponent<SegmentPicker> Render(string? value, Action<string?>? onChange = null, bool disabled = false) =>
        RenderComponent<SegmentPicker>(p =>
        {
            p.Add(c => c.Value, value).Add(c => c.Presets, Presets).Add(c => c.Disabled, disabled);
            if (onChange is not null) p.Add(c => c.ValueChanged, (string? v) => onChange(v));
        });

    private static string[] Names(IRenderedComponent<SegmentPicker> cut) =>
        cut.FindAll("[data-bob-part='name']").Select(n => n.TextContent).ToArray();

    [Fact]
    public void PresetChips_ExposePressedState_AndToggle()
    {
        string? got = null;
        var cut = Render("Back", v => got = v);
        var chips = cut.FindAll("[data-bob-part='chip']");
        Assert.Equal("true", chips.Single(c => c.TextContent == "Back").GetAttribute("aria-pressed"));
        Assert.Equal("false", chips.Single(c => c.TextContent == "Quads").GetAttribute("aria-pressed"));
        chips.Single(c => c.TextContent == "Quads").Click();
        Assert.Equal("Back\nQuads", got);
        cut.FindAll("[data-bob-part='chip']").Single(c => c.TextContent == "Back").Click();
        Assert.Equal("Quads", got);
    }

    [Fact]
    public void ValueFromParent_IsShownInOrder_WithoutDuplicates()
    {
        var cut = Render("Calves\nBack\ncalves\n\n Quads ");
        Assert.Equal(new[] { "Calves", "Back", "Quads" }, Names(cut));
    }

    [Fact]
    public void CustomArea_AddsOnEnter_AndIgnoresDuplicates()
    {
        string? got = null;
        var cut = Render("Back", v => got = v);
        cut.Find("[data-bob-part='input']").Input("Neck");
        cut.Find("[data-bob-part='input']").KeyDown(new KeyboardEventArgs { Key = "Enter" });
        Assert.Equal("Back\nNeck", got);
        cut.Find("[data-bob-part='input']").Input("back");
        cut.Find("[data-bob-part='add-button']").Click();
        Assert.Equal("Back\nNeck", got);
    }

    [Fact]
    public void Keyboard_PickUp_AndMove_Reorders()
    {
        string? got = null;
        var cut = Render("Back\nQuads\nCalves", v => got = v);
        cut.FindAll("[data-bob-part='handle']")[2].Click();
        Assert.Equal("true", cut.FindAll("[data-bob-part='handle']")[2].GetAttribute("aria-pressed"));
        cut.FindAll("[data-bob-part='handle']")[2].KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Assert.Equal("Back\nCalves\nQuads", got);
        Assert.Equal(new[] { "Back", "Calves", "Quads" }, Names(cut));
    }

    [Fact]
    public void Move_WithoutPickingUp_DoesNothing()
    {
        string? got = null;
        var cut = Render("Back\nQuads", v => got = v);
        cut.FindAll("[data-bob-part='handle']")[1].KeyDown(new KeyboardEventArgs { Key = "ArrowUp" });
        Assert.Null(got);
    }

    [Fact]
    public void Remove_DropsTheArea()
    {
        string? got = null;
        var cut = Render("Back\nQuads", v => got = v);
        cut.FindAll("[data-bob-part='remove']")[0].Click();
        Assert.Equal("Quads", got);
    }

    [Fact]
    public void Changes_AreAnnounced()
    {
        var cut = Render("Back");
        cut.FindAll("[data-bob-part='chip']").Single(c => c.TextContent == "Quads").Click();
        var status = cut.Find("[data-bob-part='status']");
        Assert.Equal("status", status.GetAttribute("role"));
        Assert.Equal("Quads added.", status.TextContent);
    }

    [Fact]
    public void GripAndRemove_AreNamedAfterTheArea()
    {
        var cut = Render("Back");
        Assert.Contains("Reorder Back", cut.Find("[data-bob-part='handle']").GetAttribute("aria-label"));
        Assert.Equal("Remove Back", cut.Find("[data-bob-part='remove']").GetAttribute("aria-label"));
    }

    [Fact]
    public void Disabled_DisablesEveryControl()
    {
        var cut = Render("Back", disabled: true);
        Assert.All(cut.FindAll("button, input"), e => Assert.True(e.HasAttribute("disabled")));
    }

    [Fact]
    public void ParentValueChange_ReplacesTheList()
    {
        var cut = Render("Back");
        cut.SetParametersAndRender(p => p.Add(c => c.Value, "Quads\nCalves"));
        Assert.Equal(new[] { "Quads", "Calves" }, Names(cut));
    }
}
