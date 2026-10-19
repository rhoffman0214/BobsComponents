using Bunit;
using BobsComponents.Components;
using BobsComponents.Infrastructure;
using BobsComponents.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace BobsComponents.Tests.Components;

public class NotificationBellTests : TestContext
{
    private static readonly DateTimeOffset Now = new(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);

    private sealed class FakeSource : IBobNotificationSource
    {
        public int Unread = 3;
        public int MarkCalls;
        public bool Fail;
        public List<BobNotification> Items = [];

        public Task<int> GetUnreadCountAsync() => Fail ? throw new HttpRequestException("down") : Task.FromResult(Unread);
        public Task<IReadOnlyList<BobNotification>> ListAsync() => Task.FromResult<IReadOnlyList<BobNotification>>(Items);
        public Task MarkAllReadAsync() { MarkCalls++; return Task.CompletedTask; }
    }

    private readonly FakeSource _source = new();

    public NotificationBellTests()
    {
        Services.AddBobComponents();
        Services.AddSingleton<TimeProvider>(new ManualTimeProvider(Now));
        JSInterop.Mode = JSRuntimeMode.Loose;
        _source.Items =
        [
            new("Booked", Now.AddMinutes(-5), "/b/1"),
            new("Plan updated", Now.AddDays(-2)),
        ];
    }

    private IRenderedComponent<NotificationBell> Render() =>
        RenderComponent<NotificationBell>(p => p.Add(c => c.Source, _source));

    [Fact]
    public void ButtonNameIncludesUnreadCount()
    {
        var cut = Render();
        var button = cut.Find("[data-bob-part='button']");
        Assert.Equal("Notifications, 3 unread", button.GetAttribute("aria-label"));
        Assert.Equal("false", button.GetAttribute("aria-expanded"));
        Assert.Equal("true", cut.Find("[data-bob-part='badge']").GetAttribute("aria-hidden"));
    }

    [Fact]
    public void NoUnread_HasNoBadgeAndPlainName()
    {
        _source.Unread = 0;
        var cut = Render();
        Assert.Empty(cut.FindAll("[data-bob-part='badge']"));
        Assert.Equal("Notifications", cut.Find("button").GetAttribute("aria-label"));
    }

    [Fact]
    public void LargeCounts_ShowNinePlus()
    {
        _source.Unread = 25;
        Assert.Equal("9+", Render().Find("[data-bob-part='badge']").TextContent);
    }

    [Fact]
    public void OpeningLoadsListAndMarksRead()
    {
        var cut = Render();
        cut.Find("[data-bob-part='button']").Click();
        Assert.Equal("true", cut.Find("[data-bob-part='button']").GetAttribute("aria-expanded"));
        Assert.Equal(2, cut.FindAll("[data-bob-part='item']").Count);
        Assert.Equal(1, _source.MarkCalls);
        Assert.Empty(cut.FindAll("[data-bob-part='badge']"));
        Assert.Equal(new[] { "5m ago", "2d ago" },
            cut.FindAll(".bob-notification-bell__time").Select(t => t.TextContent).ToArray());
    }

    [Fact]
    public void EmptyList_ShowsEmptyText()
    {
        _source.Items = [];
        var cut = Render();
        cut.Find("[data-bob-part='button']").Click();
        Assert.Equal("Nothing yet.", cut.Find("[data-bob-part='status']").TextContent);
    }

    [Fact]
    public void Escape_ClosesPanel()
    {
        var cut = Render();
        cut.Find("[data-bob-part='button']").Click();
        cut.Find("[data-bob-part='panel']").KeyDown(new KeyboardEventArgs { Key = "Escape" });
        Assert.Empty(cut.FindAll("[data-bob-part='panel']"));
        Assert.Equal("false", cut.Find("[data-bob-part='button']").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void Backdrop_ClosesPanel()
    {
        var cut = Render();
        cut.Find("[data-bob-part='button']").Click();
        cut.Find("[data-bob-part='backdrop']").Click();
        Assert.Empty(cut.FindAll("[data-bob-part='panel']"));
    }

    [Fact]
    public void LinkedEntry_RaisesOnNavigateAndCloses()
    {
        string? url = null;
        var cut = RenderComponent<NotificationBell>(p => p
            .Add(c => c.Source, _source)
            .Add(c => c.OnNavigate, (string u) => url = u));
        cut.Find("[data-bob-part='button']").Click();
        Assert.Single(cut.FindAll("[data-bob-part='link']"));
        cut.Find("[data-bob-part='link']").Click();
        Assert.Equal("/b/1", url);
        Assert.Empty(cut.FindAll("[data-bob-part='panel']"));
    }

    [Fact]
    public async Task RefreshAsync_PicksUpNewUnread()
    {
        _source.Unread = 0;
        var cut = Render();
        _source.Unread = 2;
        await cut.InvokeAsync(() => cut.Instance.RefreshAsync());
        Assert.Equal("Notifications, 2 unread", cut.Find("button").GetAttribute("aria-label"));
    }

    [Fact]
    public void FailedLoad_KeepsRenderingWithoutCount()
    {
        _source.Fail = true;
        var cut = Render();
        Assert.Equal("Notifications", cut.Find("button").GetAttribute("aria-label"));
    }

    [Fact]
    public void RegisteredService_IsUsedWhenNoSourceGiven()
    {
        Services.AddSingleton<IBobNotificationSource>(_source);
        var cut = RenderComponent<NotificationBell>();
        Assert.Equal("Notifications, 3 unread", cut.Find("button").GetAttribute("aria-label"));
    }
}
