using BobsComponents.Docs.Generation;

namespace BobsComponents.Tests.Docs;

/// <summary>Accessibility guarantees that live in the stylesheets rather than in a component's markup.</summary>
public class SharedAccessibilityTests
{
    [Fact]
    public void FocusVisibleRule_AppliesToEveryBobElement_WithAnOutline()
    {
        var rule = CssParser.Parse("css/base.css", DocSources.Css("css/base.css")).Rules
            .Single(r => r.Selectors.Any(s => s.Subject.Pseudos.Contains(":focus-visible")));
        Assert.Contains(rule.Selectors, s => s.Subject.Attributes.Any(a => a.StartsWith("class*=\"bob-\"")));
        Assert.Contains(rule.Declarations, d => d.Property == "outline" && d.Value.Contains("--bob-color-focus"));
    }

    [Fact]
    public void ButtonMinimumHeight_IsAtLeastA44PixelTouchTarget()
    {
        var button = CssParser.Parse("css/components/button.css", DocSources.Css("css/components/button.css"));
        var height = button.Rules.SelectMany(r => r.Declarations)
            .Single(d => d.Property == "min-height" && d.VarRefs.Any(v => v.Name == "--bob-button-min-height"));
        var fallback = height.VarRefs.First(v => v.Name == "--bob-button-min-height").Fallback!;
        Assert.EndsWith("rem", fallback);
        Assert.True(double.Parse(fallback[..^3], System.Globalization.CultureInfo.InvariantCulture) * 16 >= 44);
    }
}
