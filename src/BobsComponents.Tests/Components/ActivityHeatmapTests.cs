using Bunit;
using BobsComponents.Components;
using BobsComponents.Infrastructure;

namespace BobsComponents.Tests.Components;

public class ActivityHeatmapTests : TestContext
{
    private static readonly DateOnly Today = new(2026, 10, 18); // a Sunday

    public ActivityHeatmapTests() => Services.AddBobComponents();

    private static DateTimeOffset At(int daysAgo) =>
        new(Today.AddDays(-daysAgo).ToDateTime(new TimeOnly(9, 0)), TimeSpan.Zero);

    private IRenderedComponent<ActivityHeatmap> Render(params int[] daysAgo) =>
        RenderComponent<ActivityHeatmap>(p => p
            .Add(c => c.Timestamps, daysAgo.Select(At).ToArray())
            .Add(c => c.Today, Today));

    private static string[] Values(IRenderedComponent<ActivityHeatmap> cut) =>
        cut.FindAll(".bob-activity-heatmap__value").Select(v => v.TextContent).ToArray();

    [Fact]
    public void NullTimestamps_ShowsLoadingStatus()
    {
        var cut = RenderComponent<ActivityHeatmap>();
        Assert.Equal("status", cut.Find("[data-bob-part='loading']").GetAttribute("role"));
        Assert.Empty(cut.FindAll("[data-bob-part='grid']"));
    }

    [Fact]
    public void StatsAreTextPairedWithLabels()
    {
        var cut = Render(0, 1, 2, 5);
        Assert.Equal(new[] { "3", "3", "4" }, Values(cut));
        Assert.Equal(new[] { "Day streak", "Longest streak", "Active days" },
            cut.FindAll("dt").Select(t => t.TextContent).ToArray());
    }

    [Fact]
    public void CurrentStreak_SurvivesAnEmptyToday()
    {
        Assert.Equal("2", Values(Render(1, 2, 5))[0]);
    }

    [Fact]
    public void CurrentStreak_BreaksAfterAMissedDay()
    {
        var v = Values(Render(2, 3, 4));
        Assert.Equal("0", v[0]);
        Assert.Equal("3", v[1]);
    }

    [Fact]
    public void GridHasTextSummary()
    {
        var grid = Render(0, 0, 1).Find("[data-bob-part='grid']");
        Assert.Equal("img", grid.GetAttribute("role"));
        Assert.Contains("3 in the last 18 weeks on 2 days", grid.GetAttribute("aria-label"));
    }

    [Fact]
    public void BuildsWholeWeeks_AndHidesFutureDays()
    {
        var cut = Render(0);
        Assert.Equal(18, cut.FindAll("[data-bob-part='week']").Count);
        Assert.Equal(18 * 7, cut.FindAll("[data-bob-part='cell']").Count);
        // Today is a Sunday, so the last week has six future days left empty.
        Assert.Equal(6, cut.FindAll(".bob-activity-heatmap__cell--empty").Count);
    }

    [Fact]
    public void LevelsCapAtThree()
    {
        var cut = Render(0, 0, 0, 0, 0, 1);
        var levels = cut.FindAll("[data-level]").Select(c => c.GetAttribute("data-level")).Where(l => l != "").ToList();
        Assert.Contains("3", levels);
        Assert.Contains("1", levels);
        Assert.DoesNotContain("5", levels);
    }

    [Fact]
    public void InvalidWeeks_ReportsAParameterError()
    {
        Assert.ThrowsAny<Exception>(() => RenderComponent<ActivityHeatmap>(p => p
            .Add(c => c.Timestamps, Array.Empty<DateTimeOffset>()).Add(c => c.Weeks, 0)));
    }
}
