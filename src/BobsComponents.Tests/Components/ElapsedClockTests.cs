using Bunit;
using BobsComponents.Components;
using BobsComponents.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BobsComponents.Tests.Components;

public class ElapsedClockTests : TestContext
{
    private static readonly DateTimeOffset Start = new(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);
    private readonly ManualTimeProvider _time = new(Start);

    public ElapsedClockTests()
    {
        Services.AddBobComponents();
        Services.AddSingleton<TimeProvider>(_time);
    }

    [Fact]
    public void IsATimer_ThatDoesNotAnnounceEverySecond()
    {
        var cut = RenderComponent<ElapsedClock>(p => p.Add(c => c.Since, Start));
        var root = cut.Find("[data-bob-part='root']");
        Assert.Equal("timer", root.GetAttribute("role"));
        Assert.Equal("off", root.GetAttribute("aria-live"));
    }

    [Fact]
    public void FormatsMinutesAndSeconds_ThenHours()
    {
        var cut = RenderComponent<ElapsedClock>(p => p.Add(c => c.Since, Start.AddSeconds(-65)));
        Assert.Equal("01:05", cut.Find("span").TextContent);

        var long_ = RenderComponent<ElapsedClock>(p => p.Add(c => c.Since, Start.AddMinutes(-75).AddSeconds(-9)));
        Assert.Equal("1:15:09", long_.Find("span").TextContent);
    }

    [Fact]
    public void TicksEachSecond()
    {
        var cut = RenderComponent<ElapsedClock>(p => p.Add(c => c.Since, Start));
        Assert.Equal("00:00", cut.Find("span").TextContent);
        _time.Advance(TimeSpan.FromSeconds(3));
        cut.WaitForAssertion(() => Assert.Equal("00:03", cut.Find("span").TextContent));
    }

    [Fact]
    public void PausedClock_DoesNotAdvance()
    {
        var cut = RenderComponent<ElapsedClock>(p => p.Add(c => c.Since, Start).Add(c => c.Paused, true));
        _time.Advance(TimeSpan.FromSeconds(10));
        Assert.Equal("00:00", cut.Find("span").TextContent);

        cut.SetParametersAndRender(p => p.Add(c => c.Paused, false));
        Assert.Equal("00:10", cut.Find("span").TextContent);
    }

    [Fact]
    public void FutureStart_ShowsZero_NotNegative()
    {
        var cut = RenderComponent<ElapsedClock>(p => p.Add(c => c.Since, Start.AddMinutes(5)));
        Assert.Equal("00:00", cut.Find("span").TextContent);
    }

    [Fact]
    public void DisposingStopsTheTimer()
    {
        var cut = RenderComponent<ElapsedClock>(p => p.Add(c => c.Since, Start));
        Assert.Equal(1, _time.ActiveTimers);
        DisposeComponents();
        Assert.Equal(0, _time.ActiveTimers);
    }

    [Fact]
    public void ExtraClassAndAttributesApply()
    {
        var cut = RenderComponent<ElapsedClock>(p => p.Add(c => c.Since, Start).Add(c => c.CssClass, "mine").AddUnmatched("data-x", "1"));
        var root = cut.Find("[data-bob-part='root']");
        Assert.Contains("mine", root.ClassName);
        Assert.Equal("1", root.GetAttribute("data-x"));
    }
}
