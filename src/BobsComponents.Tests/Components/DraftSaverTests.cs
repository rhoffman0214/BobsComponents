using Bunit;
using BobsComponents.Components;
using BobsComponents.Infrastructure;
using BobsComponents.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BobsComponents.Tests.Components;

public class DraftSaverTests : TestContext
{
    private readonly BobMemoryDraftStore _store = new();

    public DraftSaverTests()
    {
        Services.AddBobComponents();
        Services.AddSingleton<IBobDraftStore>(_store);
    }

    [Fact]
    public void NoSavedDraft_RendersAnEmptyStatus()
    {
        var cut = RenderComponent<DraftSaver>(p => p.Add(c => c.Name, "a"));
        Assert.Equal("status", cut.Find("[data-bob-part='status']").GetAttribute("role"));
        Assert.Equal("", cut.Find("[data-bob-part='status']").TextContent);
    }

    [Fact]
    public async Task RestoresOnce_AndAnnounces()
    {
        await _store.SaveAsync("a", "hello");
        var restored = new List<string>();
        var cut = RenderComponent<DraftSaver>(p => p
            .Add(c => c.Name, "a")
            .Add(c => c.OnRestore, (string s) => restored.Add(s)));
        cut.WaitForAssertion(() => Assert.Equal("Restored your unsaved changes.", cut.Find("[data-bob-part='status']").TextContent));
        cut.SetParametersAndRender(p => p.Add(c => c.Value, "hello"));
        Assert.Single(restored);
        Assert.Equal("hello", restored[0]);
    }

    [Fact]
    public void ChangedValue_IsSaved_AndNullClears()
    {
        var cut = RenderComponent<DraftSaver>(p => p.Add(c => c.Name, "a"));
        cut.SetParametersAndRender(p => p.Add(c => c.Value, "one"));
        cut.WaitForAssertion(() => Assert.Equal("one", _store.LoadAsync("a").AsTask().Result));
        cut.SetParametersAndRender(p => p.Add(c => c.Value, (string?)null));
        cut.WaitForAssertion(() => Assert.Null(_store.LoadAsync("a").AsTask().Result));
    }

    [Fact]
    public async Task EmptyForm_DoesNotWipeAPendingDraft()
    {
        await _store.SaveAsync("a", "keep me");
        RenderComponent<DraftSaver>(p => p.Add(c => c.Name, "a").Add(c => c.Value, (string?)null));
        Assert.Equal("keep me", await _store.LoadAsync("a"));
    }
}
