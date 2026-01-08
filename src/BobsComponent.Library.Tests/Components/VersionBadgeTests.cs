using Bunit;
using Xunit;
using BobsComponent.Library;
using BobsComponent.Library.Components;

namespace BobsComponent.Library.Tests.Components;

public class VersionBadgeTests : TestContext
{
    [Fact]
    public void VersionBadge_RendersSpanElement()
    {
        var cut = RenderComponent<VersionBadge>();

        var span = cut.Find("span.version-badge");
        Assert.NotNull(span);
    }

    [Fact]
    public void VersionBadge_DisplaysCorrectVersion()
    {
        var cut = RenderComponent<VersionBadge>();

        var span = cut.Find("span.version-badge");
        var text = span.TextContent.Trim();

        // Should display "v0.1" not "v@AppVersion.Version"
        Assert.Equal("v0.1", text);
    }

    [Fact]
    public void VersionBadge_VersionMatchesAppVersionConstant()
    {
        var cut = RenderComponent<VersionBadge>();

        var span = cut.Find("span.version-badge");
        var text = span.TextContent.Trim();

        // Should match the AppVersion.Version constant
        Assert.Equal($"v{AppVersion.Version}", text);
    }

    [Fact]
    public void VersionBadge_HasCorrectCssClass()
    {
        var cut = RenderComponent<VersionBadge>();

        var span = cut.Find("span.version-badge");
        Assert.Contains("version-badge", span.ClassName);
    }

    [Fact]
    public void VersionBadge_HasTestId()
    {
        var cut = RenderComponent<VersionBadge>();

        var span = cut.Find("[data-testid='version-badge']");
        Assert.NotNull(span);
    }

    [Fact]
    public void VersionBadge_DoesNotDisplayVariableName()
    {
        var cut = RenderComponent<VersionBadge>();

        var span = cut.Find("span.version-badge");
        var text = span.TextContent.Trim();

        // Should NOT contain "@AppVersion" or "AppVersion.Version"
        Assert.DoesNotContain("@AppVersion", text);
        Assert.DoesNotContain("AppVersion.Version", text);
    }

    [Fact]
    public void VersionBadge_StartsWithVersionPrefix()
    {
        var cut = RenderComponent<VersionBadge>();

        var span = cut.Find("span.version-badge");
        var text = span.TextContent.Trim();

        Assert.StartsWith("v", text);
    }
}
