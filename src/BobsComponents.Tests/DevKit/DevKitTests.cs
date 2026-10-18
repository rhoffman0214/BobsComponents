using System.Net;
using Bunit;
using BobsComponents.Components;
using BobsComponents.DevKit;
using BobsComponents.Errors;
using BobsComponents.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BobsComponents.Tests.DevKit;

internal interface IUnregisteredDep { }

[BobRequiresService(typeof(IUnregisteredDep))]
internal sealed class NeedyComponent { }

public class DevKitTests : TestContext
{
    [Fact]
    public void OffByDefault_WhenEnvironmentIsNotDevelopment()
    {
        Services.AddBobComponents(o => o.EnvironmentName = "Production");
        var cut = RenderComponent<BobDevKit>();
        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void OffWhenEnvironmentUnknown()
    {
        Services.AddBobComponents(o => o.EnvironmentName = null);
        var cut = RenderComponent<BobDevKit>();
        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void OnInDevelopment()
    {
        Services.AddBobComponents(o => o.EnvironmentName = "Development");
        var cut = RenderComponent<BobDevKit>();
        Assert.NotEmpty(cut.FindAll("[data-bob-part='toggle']"));
    }

    [Fact]
    public void EnabledOption_OverridesEnvironment_BothWays()
    {
        Services.AddBobComponents(o => { o.EnvironmentName = "Production"; o.DevKit.Enabled = true; });
        var cut = RenderComponent<BobDevKit>();
        Assert.NotEmpty(cut.FindAll("[data-bob-part='toggle']"));
    }

    [Fact]
    public void EnabledParameter_ForcesOff_EvenInDevelopment()
    {
        Services.AddBobComponents(o => o.EnvironmentName = "Development");
        var cut = RenderComponent<BobDevKit>(p => p.Add(c => c.Enabled, false));
        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void ErrorsPanel_ShowsErrorsNoBoundaryCaught_WithExactMessage()
    {
        Services.AddBobComponents(o => o.EnvironmentName = "Development");
        var cut = RenderComponent<BobDevKit>(p => p.Add(c => c.Enabled, true));
        var hub = Services.GetRequiredService<IBobErrorHub>();

        cut.Find("[data-bob-part='toggle']").Click();
        Assert.Contains("No errors yet", cut.Markup);

        hub.Report(new HttpRequestException("connection refused"), "FireAndForget");
        cut.WaitForAssertion(() =>
        {
            Assert.Equal("connection refused", cut.Find("[data-bob-part='message']").TextContent);
            Assert.Equal("1", cut.Find("[data-bob-part='count']").TextContent.Trim());
        });
    }

    [Fact]
    public async Task NetworkHandler_ShowsFailedRequest_WithServersOwnMessage_AndReportsError()
    {
        Services.AddBobComponents(o => o.EnvironmentName = "Development");
        var log = Services.GetRequiredService<BobNetworkLog>();
        var hub = Services.GetRequiredService<IBobErrorHub>();
        var handler = new BobNetworkHandler(log, Services.GetRequiredService<BobOptions>(), hub)
        {
            InnerHandler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                ReasonPhrase = "Bad Request",
                Content = new StringContent("{\"detail\":\"Quantity must be positive.\"}"),
            }),
        };
        using var client = new HttpClient(handler);

        var response = await client.PostAsync("https://example.test/api/order", new StringContent("{\"qty\":-1}"));

        // The app can still read the body after the dev kit did.
        Assert.Contains("Quantity must be positive.", await response.Content.ReadAsStringAsync());
        var entry = Assert.Single(log.Items);
        Assert.Equal(400, entry.Status);
        Assert.Equal("{\"qty\":-1}", entry.RequestBody);
        Assert.Contains("Quantity must be positive.", entry.ResponseBody);
        var error = Assert.Single(hub.Errors);
        Assert.Equal(BobErrorOrigin.NetworkFailure, error.Origin);
        Assert.Equal("Quantity must be positive.", error.Message);
        Assert.Equal(400, error.StatusCode);

        var cut = RenderComponent<BobDevKit>(p => p.Add(c => c.Enabled, true));
        cut.Find("[data-bob-part='toggle']").Click();
        cut.FindAll("[data-bob-part='tab']").First(t => t.TextContent == "Network").Click();
        Assert.Contains("400", cut.Find("[data-bob-part='entry'] summary").TextContent);
        Assert.Contains("Quantity must be positive.", cut.Markup);
    }

    [Fact]
    public async Task NetworkHandler_TransportFailure_IsLoggedAndReported()
    {
        Services.AddBobComponents(o => o.EnvironmentName = "Development");
        var log = Services.GetRequiredService<BobNetworkLog>();
        var hub = Services.GetRequiredService<IBobErrorHub>();
        var handler = new BobNetworkHandler(log, Services.GetRequiredService<BobOptions>(), hub)
        {
            InnerHandler = new StubHandler(_ => throw new HttpRequestException("No such host is known.")),
        };
        using var client = new HttpClient(handler);

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetAsync("https://nowhere.test/"));

        Assert.Equal("No such host is known.", Assert.Single(log.Items).TransportError);
        Assert.Equal("BOB-NET-002", Assert.Single(hub.Errors).Code);
    }

    [Fact]
    public async Task NetworkHandler_DoesNothing_WhenDevKitOff_AndNeverRecordsHeaders()
    {
        Services.AddBobComponents(o => o.EnvironmentName = "Production");
        var log = Services.GetRequiredService<BobNetworkLog>();
        var handler = new BobNetworkHandler(log, Services.GetRequiredService<BobOptions>())
        {
            InnerHandler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)),
        };
        using var client = new HttpClient(handler);
        await client.GetAsync("https://example.test/");
        Assert.Empty(log.Items);

        Assert.DoesNotContain(typeof(BobNetworkEntry).GetProperties(), p => p.Name.Contains("Header"));
    }

    [Fact]
    public async Task InteropLogger_RecordsCalls_AndReportsMissingScriptByName()
    {
        Services.AddBobComponents(o => o.EnvironmentName = "Development");
        var log = Services.GetRequiredService<BobInteropLog>();
        var hub = Services.GetRequiredService<IBobErrorHub>();
        var js = new BobLoggingJSRuntime(new FakeJs(), log, hub);

        await js.InvokeVoidAsync("document.title.get", "a", 2);
        Assert.Equal("document.title.get", Assert.Single(log.Items).Identifier);
        Assert.Equal("\"a\", 2", log.Items[0].Arguments);

        await Assert.ThrowsAsync<JSException>(async () => await js.InvokeAsync<IJSObjectReference>("import", "./_content/BobsComponents/js/missing.js"));
        var error = Assert.Single(hub.Errors);
        Assert.Equal("BOB-ASSET-001", error.Code);
        Assert.Equal("./_content/BobsComponents/js/missing.js", error.Facts["script"]);
    }

    [Fact]
    public void DiPanel_FlagsMissingServices()
    {
        Services.AddBobComponents(o => o.EnvironmentName = "Development");
        var registry = Services.GetRequiredService<BobDiRegistry>();
        registry.AddAssembly(typeof(NeedyComponent).Assembly);

        var rows = registry.Check();
        Assert.Contains(rows, r => r.Component == nameof(NeedyComponent) && !r.Registered);
        Assert.Contains(rows, r => r.Component == nameof(BobDevKit) && r.ServiceType == typeof(IBobErrorHub) && r.Registered);

        var cut = RenderComponent<BobDevKit>(p => p.Add(c => c.Enabled, true));
        cut.Find("[data-bob-part='toggle']").Click();
        cut.FindAll("[data-bob-part='tab']").First(t => t.TextContent == "DI").Click();
        Assert.NotEmpty(cut.FindAll("[data-bob-part='di-table']"));
    }

    [Fact]
    public void Panels_CanBeTurnedOff()
    {
        Services.AddBobComponents(o =>
        {
            o.DevKit.Enabled = true;
            o.DevKit.ShowNetworkPanel = false;
            o.DevKit.ShowInteropPanel = false;
            o.DevKit.ShowDiPanel = false;
            o.DevKit.StartOpen = true;
        });
        var cut = RenderComponent<BobDevKit>();
        var tabs = cut.FindAll("[data-bob-part='tab']");
        Assert.Equal(["Errors"], tabs.Select(t => t.TextContent).ToArray());
    }

    [Fact]
    public void ErrorLog_IsBounded()
    {
        Services.AddBobComponents(o => o.DevKit.MaxEntries = 3);
        var hub = Services.GetRequiredService<IBobErrorHub>();
        for (var i = 0; i < 10; i++) hub.Report(new HttpRequestException($"e{i}"));
        Assert.Equal(["e7", "e8", "e9"], hub.Errors.Select(e => e.Message).ToArray());
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            Task.FromResult(respond(request));
    }

    private sealed class FakeJs : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            identifier == "import"
                ? throw new JSException("Failed to fetch dynamically imported module")
                : new ValueTask<TValue>(default(TValue)!);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);
    }
}
