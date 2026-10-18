using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Bunit;
using BobsComponents.Components;
using BobsComponents.Errors;
using BobsComponents.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BobsComponents.Tests.Errors;

/// <summary>Test child components that fail in each of the six ways.</summary>
internal sealed class Thrower : ComponentBase
{
    [Parameter] public Func<Exception> Make { get; set; } = default!;
    protected override void OnInitialized() =>
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw(Make());
}

/// <summary>Library-style components defined here so the error tests do not depend on the UI components.</summary>
internal sealed class Sized : BobComponentBase
{
    [Parameter] public int Size { get; set; } = 10;
    protected override string ComponentName => "Sized";
    protected override void ValidateParameters(BobParameterValidator check) => check.Range(nameof(Size), Size, 1, 512);
}

internal sealed class ClickProbe : BobComponentBase
{
    [Parameter] public EventCallback OnClick { get; set; }
    protected override string ComponentName => "ClickProbe";
    protected override void BuildRenderTree(RenderTreeBuilder b)
    {
        b.OpenElement(0, "button");
        b.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, () => InvokeCallbackAsync(nameof(OnClick), OnClick)));
        b.AddContent(2, "Go");
        b.CloseElement();
    }
}

internal interface IMissingService { }

internal sealed class NeedsService : BobComponentBase
{
    protected override void OnInitialized() => RequireService<IMissingService>();
}

/// <summary>
/// One test per error origin: the boundary must show the origin badge, the code and the exact message.
/// </summary>
public class ErrorOriginTests : TestContext
{
    public ErrorOriginTests() => Services.AddBobComponents();

    private IRenderedComponent<BobErrorBoundary> RenderBoundary(RenderFragment child) =>
        RenderComponent<BobErrorBoundary>(p => p.Add(b => b.ChildContent, child));

    private IRenderedComponent<BobErrorBoundary> RenderThrowing(Func<Exception> make) =>
        RenderBoundary(b =>
        {
            b.OpenComponent<Thrower>(0);
            b.AddAttribute(1, nameof(Thrower.Make), make);
            b.CloseComponent();
        });

    private static void AssertCard(IRenderedComponent<BobErrorBoundary> cut, string badge, string code, string exactMessage)
    {
        var card = cut.Find(".bob-error-card");
        Assert.Equal(badge, cut.Find("[data-bob-part='badge']").TextContent.Trim());
        Assert.Equal(code, card.GetAttribute("data-bob-code"));
        Assert.Equal(exactMessage, cut.Find("[data-bob-part='message']").TextContent);
        Assert.Equal("alert", card.GetAttribute("role"));
    }

    [Fact]
    public void BugInComponent()
    {
        var cut = RenderThrowing(() =>
        {
            try { BobDiagnostics.ThrowSimulatedComponentBug("exact bug text"); }
            catch (Exception ex) { return ex; }
            return null!;
        });
        AssertCard(cut, "Component bug", "BOB-COMP-001", "exact bug text");
    }

    [Fact]
    public void WrongParameters_FromComponentValidation()
    {
        var cut = RenderBoundary(b =>
        {
            b.OpenComponent<Sized>(0);
            b.AddAttribute(1, nameof(Sized.Size), 0);
            b.CloseComponent();
        });
        AssertCard(cut, "Wrong parameters", "BOB-PARAM-002",
            "Sized: parameter 'Size' was 0. Expected a value from 1 to 512.");
        Assert.Contains("Size", cut.Find("[data-bob-part='facts']").TextContent);
    }

    [Fact]
    public void WrongParameters_RequiredMissing()
    {
        var cut = RenderBoundary(b =>
        {
            b.OpenComponent<BobErrorCard>(0);
            b.CloseComponent();
        });
        AssertCard(cut, "Wrong parameters", "BOB-PARAM-001",
            "BobErrorCard: required parameter 'Error' was not set. Expected a BobError from IBobErrorClassifier or IBobErrorHub.");
    }

    [Fact]
    public void DataShapeMismatch_NamesThePath()
    {
        var cut = RenderThrowing(() => new BobDataShapeException("Chart", "$.points[2].value", "number", "string \"abc\""));
        AssertCard(cut, "Data shape mismatch", "BOB-DATA-001",
            "Data shape mismatch at '$.points[2].value': expected number, got string \"abc\".");
        Assert.Contains("$.points[2].value", cut.Find("[data-bob-part='facts']").TextContent);
    }

    [Fact]
    public async Task DataShapeMismatch_FromFailedDeserialization()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"items\":[{\"count\":\"not a number\"}]}", new MediaTypeHeaderValue("application/json")),
        };
        var ex = await Assert.ThrowsAsync<BobDataShapeException>(() => BobHttp.ReadJsonAsync<Payload>(response, "Table"));
        Assert.Equal("$.items[0].count", ex.Path);

        var cut = RenderThrowing(() => ex);
        Assert.Equal("BOB-DATA-002", cut.Find(".bob-error-card").GetAttribute("data-bob-code"));
    }

    private sealed record Payload(List<Item> Items);
    private sealed record Item(int Count);

    [Fact]
    public void MissingAsset_JsException()
    {
        var cut = RenderThrowing(() => new JSException("Failed to fetch dynamically imported module"));
        AssertCard(cut, "Missing asset or service", "BOB-ASSET-001", "Failed to fetch dynamically imported module");
    }

    [Fact]
    public void MissingAsset_ScriptNamed()
    {
        var cut = RenderThrowing(() => BobMissingDependencyException.Script("ModalSheet", BobJs.FocusTrap));
        Assert.Contains(BobJs.FocusTrap, cut.Find("[data-bob-part='message']").TextContent);
        Assert.Contains(BobJs.FocusTrap, cut.Find("[data-bob-part='facts']").TextContent);
    }

    [Fact]
    public void MissingService_NamesTheService()
    {
        var cut = RenderBoundary(b =>
        {
            b.OpenComponent<NeedsService>(0);
            b.CloseComponent();
        });
        AssertCard(cut, "Missing asset or service", "BOB-ASSET-002",
            $"Service '{typeof(IMissingService).FullName}' is not registered. Register it with AddBobComponents() or your own services.");
    }

    [Fact]
    public async Task FailedNetworkCall_ShowsServerMessageVerbatim()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            ReasonPhrase = "Unprocessable Entity",
            RequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://example.test/api/items"),
            Content = new StringContent("{\"title\":\"Validation failed\",\"detail\":\"Name must be at least 3 characters.\"}",
                new MediaTypeHeaderValue("application/problem+json")),
        };
        var ex = await Assert.ThrowsAsync<BobHttpException>(() => BobHttp.EnsureSuccessAsync(response, "ItemForm"));

        var cut = RenderThrowing(() => ex);
        AssertCard(cut, "Network failure", "BOB-NET-001", "Name must be at least 3 characters.");
        var facts = cut.Find("[data-bob-part='facts']").TextContent;
        Assert.Contains("422 Unprocessable Entity", facts);
        Assert.Contains("https://example.test/api/items", facts);
    }

    [Fact]
    public void FailedNetworkCall_TransportError()
    {
        var cut = RenderThrowing(() => new HttpRequestException("No such host is known."));
        AssertCard(cut, "Network failure", "BOB-NET-002", "No such host is known.");
    }

    [Fact]
    public void FailedNetworkCall_RawStatusException()
    {
        var cut = RenderThrowing(() => new HttpRequestException("Response status code does not indicate success: 503.", null, HttpStatusCode.ServiceUnavailable));
        AssertCard(cut, "Network failure", "BOB-NET-001", "Response status code does not indicate success: 503.");
    }

    [Fact]
    public void CallbackThrew_ShowsYourMessageAndFrame()
    {
        var cut = RenderThrowing(() => new InvalidOperationException("my handler blew up"));
        AssertCard(cut, "Your callback threw", "BOB-CB-001", "my handler blew up");
        Assert.Contains("yourFrame", cut.Find("[data-bob-part='facts']").TextContent);
    }

    [Fact]
    public void CallbackThrew_EventCallbackOnRealComponent()
    {
        var cut = RenderBoundary(b =>
        {
            b.OpenComponent<ClickProbe>(0);
            b.AddAttribute(1, nameof(ClickProbe.OnClick),
                EventCallback.Factory.Create(this, () => throw new InvalidOperationException("click handler failed")));
            b.CloseComponent();
        });

        cut.Find("button").Click();

        AssertCard(cut, "Your callback threw", "BOB-CB-001", "click handler failed");
        Assert.Contains("OnClick", cut.Find("[data-bob-part='facts']").TextContent);
    }

    [Fact]
    public void ClassificationIgnoresMessageText()
    {
        // A message that talks about the network and parameters must not change a plain app exception's origin.
        var cut = RenderThrowing(() => new InvalidOperationException("HttpRequestException: parameter 'Size' network timeout 404 JSException"));
        Assert.Equal("BOB-CB-001", cut.Find(".bob-error-card").GetAttribute("data-bob-code"));
    }

    [Fact]
    public void Aggregate_And_Reflection_Wrappers_AreUnwrapped()
    {
        var classifier = new BobErrorClassifier();
        var inner = new HttpRequestException("down");
        var error = classifier.Classify(new AggregateException(inner));
        Assert.Equal(BobErrorOrigin.NetworkFailure, error.Origin);
        error = classifier.Classify(new System.Reflection.TargetInvocationException(new JsonException("bad", "$.a", 1, 1)));
        Assert.Equal(BobErrorOrigin.DataShapeMismatch, error.Origin);
        Assert.Equal("$.a", error.Facts["path"]);
    }
}
