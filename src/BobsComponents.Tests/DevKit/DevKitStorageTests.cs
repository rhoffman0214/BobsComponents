using Bunit;
using BobsComponents.DevKit;
using BobsComponents.Components;
using BobsComponents.Errors;
using BobsComponents.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BobsComponents.Tests.DevKit;

public class DevKitStorageTests : TestContext
{
    private BunitJSModuleInterop Module() => JSInterop.SetupModule(BobJs.DevKit);

    private IRenderedComponent<BobDevKit> OpenKit(Action<BobsComponents.Options.BobOptions>? configure = null)
    {
        Services.AddBobComponents(o => { o.EnvironmentName = "Development"; configure?.Invoke(o); });
        var cut = RenderComponent<BobDevKit>();
        cut.Find("[data-bob-part='toggle']").Click();
        return cut;
    }

    [Fact]
    public void StorageTab_ListsKeyNamesAndSizes_ForBothStores()
    {
        var module = Module();
        module.Setup<BobDevKit.StorageSnapshot>("storage").SetResult(new(
            [new("theme", 4)], [new("draft", 120), new("token-id", 8)]));
        var cut = OpenKit();

        cut.FindAll("[data-bob-part='tab']").Single(t => t.TextContent == "Storage").Click();

        cut.WaitForAssertion(() =>
        {
            var tables = cut.FindAll("[data-bob-part='storage-table']");
            Assert.Equal(2, tables.Count);
            Assert.Contains("theme", tables[0].TextContent);
            Assert.Contains("120", tables[1].TextContent);
        });
    }

    [Fact]
    public void StorageTab_ShowsEmptyMessage_WhenNothingStored()
    {
        Module().Setup<BobDevKit.StorageSnapshot>("storage").SetResult(new([], []));
        var cut = OpenKit();

        cut.FindAll("[data-bob-part='tab']").Single(t => t.TextContent == "Storage").Click();

        cut.WaitForAssertion(() => Assert.Equal(2, cut.FindAll("[data-bob-part='empty']").Count));
        Assert.Empty(cut.FindAll("[data-bob-part='storage-table']"));
    }

    [Fact]
    public void StorageTab_Remove_CallsJsThenReloads()
    {
        var module = Module();
        module.Setup<BobDevKit.StorageSnapshot>("storage").SetResult(new([new("theme", 4)], []));
        module.SetupVoid("removeKey", "local", "theme").SetVoidResult();
        var cut = OpenKit();
        cut.FindAll("[data-bob-part='tab']").Single(t => t.TextContent == "Storage").Click();
        cut.WaitForElement("[data-bob-part='storage-table']");

        cut.Find("[data-bob-part='storage-table'] button").Click();

        cut.WaitForAssertion(() => module.VerifyInvoke("removeKey"));
    }

    [Fact]
    public void StorageTab_CanBeTurnedOff()
    {
        var cut = OpenKit(o => o.DevKit.ShowStoragePanel = false);
        Assert.DoesNotContain(cut.FindAll("[data-bob-part='tab']"), t => t.TextContent == "Storage");
    }

    [Fact]
    public void CopyReport_WritesPlainTextReport_AndConfirms()
    {
        var module = Module();
        var copy = module.Setup<bool>("copyText", _ => true);
        copy.SetResult(true);
        var cut = OpenKit();

        cut.Find("[data-bob-part='copy-report']").Click();

        cut.WaitForAssertion(() => Assert.Equal("Copied", cut.Find("[data-bob-part='copy-report']").TextContent));
        var text = (string)copy.Invocations.Single().Arguments[0]!;
        Assert.StartsWith("Bob dev kit report", text);
    }

    [Fact]
    public void CopyReport_SaysFailed_WhenClipboardRefuses()
    {
        Module().Setup<bool>("copyText", _ => true).SetResult(false);
        var cut = OpenKit();

        cut.Find("[data-bob-part='copy-report']").Click();

        cut.WaitForAssertion(() => Assert.Equal("Copy failed", cut.Find("[data-bob-part='copy-report']").TextContent));
    }

    [Fact]
    public void Report_ListsEveryPanel_AndOmitsHeadersAndValues()
    {
        var error = new BobErrorClassifier().Classify(new InvalidOperationException("boom"), "test");
        var net = new BobNetworkEntry(DateTimeOffset.UtcNow, "GET", "https://x.test/a", 500, null, TimeSpan.FromMilliseconds(12), null, null, null);
        var report = BobDevKitReport.Build([error], [net], [], []);

        Assert.Contains("Errors (1)", report);
        Assert.Contains("boom", report);
        Assert.Contains("GET 500 https://x.test/a 12 ms", report);
        Assert.Contains("Network (1)", report);
        Assert.Contains("Interop (0)", report);
        Assert.Contains("DI (0)", report);
        Assert.DoesNotContain("Authorization", report);
    }
}
