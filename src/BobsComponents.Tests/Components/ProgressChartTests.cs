using Bunit;
using BobsComponents.Components;
using BobsComponents.Infrastructure;

namespace BobsComponents.Tests.Components;

public class ProgressChartTests : TestContext
{
    private static readonly DateTimeOffset Start = new(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);

    public ProgressChartTests() => Services.AddBobComponents();

    private static BobChartPoint[] Series() =>
        [new(Start.AddDays(10), 20), new(Start, 10), new(Start.AddDays(5), 30)];

    [Fact]
    public void PlotIsANamedImage()
    {
        var cut = RenderComponent<ProgressChart>(p => p.Add(c => c.Points, Series()).Add(c => c.Label, "Squat"));
        var plot = cut.Find("[data-bob-part='plot']");
        Assert.Equal("img", plot.GetAttribute("role"));
        Assert.Equal("Squat", plot.GetAttribute("aria-label"));
    }

    [Fact]
    public void DataIsAlsoATable_InDateOrder()
    {
        var cut = RenderComponent<ProgressChart>(p => p.Add(c => c.Points, Series()));
        Assert.Equal(2, cut.FindAll("th[scope='col']").Count);
        var values = cut.FindAll("tbody tr").Select(r => r.QuerySelectorAll("td")[1].TextContent).ToArray();
        Assert.Equal(new[] { "10", "30", "20" }, values);
    }

    [Fact]
    public void FewerThanTwoPoints_ShowsEmptyText()
    {
        var cut = RenderComponent<ProgressChart>(p => p.Add(c => c.Points, new[] { new BobChartPoint(Start, 1) }));
        Assert.Equal("Not enough data yet.", cut.Find("[data-bob-part='empty']").TextContent);
        Assert.Empty(cut.FindAll("svg"));
    }

    [Fact]
    public void AxesShowRangeAsText()
    {
        var cut = RenderComponent<ProgressChart>(p => p.Add(c => c.Points, Series()));
        var y = cut.FindAll("[data-bob-part='yaxis'] span").Select(s => s.TextContent).ToArray();
        Assert.Equal(new[] { "30", "10" }, y);
        Assert.Equal(2, cut.FindAll("[data-bob-part='xaxis'] span").Count);
    }

    [Fact]
    public void DrawsOnePointPerValue_WithinTheViewBox()
    {
        var cut = RenderComponent<ProgressChart>(p => p.Add(c => c.Points, Series()));
        var points = cut.FindAll("[data-bob-part='point']");
        Assert.Equal(3, points.Count);
        Assert.Equal("0", points[0].GetAttribute("cx"));
        Assert.Equal("300", points[2].GetAttribute("cx"));
        Assert.Equal("120", points[0].GetAttribute("cy"));
        Assert.Equal("0", points[1].GetAttribute("cy"));
    }

    [Fact]
    public void FlatSeries_DrawsAMiddleLine()
    {
        var cut = RenderComponent<ProgressChart>(p => p.Add(c => c.Points, new[] { new BobChartPoint(Start, 5), new BobChartPoint(Start.AddDays(1), 5) }));
        Assert.All(cut.FindAll("[data-bob-part='point']"), c => Assert.Equal("60", c.GetAttribute("cy")));
    }
}
