using Bunit;
using Xunit;
using BobsComponent.Library.Components.Icons;

namespace BobsComponent.Library.Tests.Components.Icons;

public class LucideIconTests : TestContext
{
    [Fact]
    public void LucideIcon_WithHomeName_RendersSvgElement()
    {
        var cut = RenderComponent<LucideIcon>(parameters => parameters
            .Add(p => p.Name, "home"));

        var svg = cut.Find("svg");
        Assert.NotNull(svg);
        Assert.Equal("lucide-icon", svg.GetAttribute("class"));
    }

    [Theory]
    [InlineData("home")]
    [InlineData("bolt")]
    [InlineData("book")]
    [InlineData("warning")]
    [InlineData("list")]
    [InlineData("code")]
    [InlineData("chevron-down")]
    public void LucideIcon_WithValidName_RendersSvgWithPath(string iconName)
    {
        var cut = RenderComponent<LucideIcon>(parameters => parameters
            .Add(p => p.Name, iconName));

        var svg = cut.Find("svg");
        Assert.NotNull(svg);
        var paths = cut.FindAll("path, line, polyline, circle");
        Assert.NotEmpty(paths);
    }

    [Fact]
    public void LucideIcon_WithCustomSize_AppliesWidthAndHeight()
    {
        var cut = RenderComponent<LucideIcon>(parameters => parameters
            .Add(p => p.Name, "home")
            .Add(p => p.Size, 32));

        var svg = cut.Find("svg");
        Assert.Equal("32", svg.GetAttribute("width"));
        Assert.Equal("32", svg.GetAttribute("height"));
    }

    [Fact]
    public void LucideIcon_WithDefaultSize_Uses24px()
    {
        var cut = RenderComponent<LucideIcon>(parameters => parameters
            .Add(p => p.Name, "home"));

        var svg = cut.Find("svg");
        Assert.Equal("24", svg.GetAttribute("width"));
        Assert.Equal("24", svg.GetAttribute("height"));
    }

    [Fact]
    public void LucideIcon_HasCorrectSvgAttributes()
    {
        var cut = RenderComponent<LucideIcon>(parameters => parameters
            .Add(p => p.Name, "home"));

        var svg = cut.Find("svg");
        Assert.Equal("http://www.w3.org/2000/svg", svg.GetAttribute("xmlns"));
        Assert.Equal("0 0 24 24", svg.GetAttribute("viewBox"));
        Assert.Equal("none", svg.GetAttribute("fill"));
        Assert.Equal("currentColor", svg.GetAttribute("stroke"));
    }
}
