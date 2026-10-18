using Bunit;
using BobsComponents.Components;
using BobsComponents.Errors;
using BobsComponents.Infrastructure;
using BobsComponents.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BobsComponents.Tests.Components;

public class LoadingSpinnerTests : TestContext
{
    public LoadingSpinnerTests() => Services.AddBobComponents();

    [Fact]
    public void IsAPoliteStatusRegion_WithDecorativeIcon()
    {
        var cut = RenderComponent<LoadingSpinner>();
        var root = cut.Find("[data-bob-part='root']");
        Assert.Equal("status", root.GetAttribute("role"));
        Assert.Equal("polite", root.GetAttribute("aria-live"));
        Assert.Equal("true", cut.Find("svg").GetAttribute("aria-hidden"));
        Assert.Equal("Loading…", cut.Find("[data-bob-part='label']").TextContent);
    }

    [Fact]
    public void HiddenLabel_IsStillInTheDom_ForScreenReaders()
    {
        var cut = RenderComponent<LoadingSpinner>(p => p.Add(c => c.ShowLabel, false).Add(c => c.Label, "Saving"));
        var label = cut.Find("[data-bob-part='label']");
        Assert.Contains("bob-visually-hidden", label.ClassName);
        Assert.Equal("Saving", label.TextContent);
    }

    [Fact]
    public void SizeAndExtraClassAndAttributesApply()
    {
        var cut = RenderComponent<LoadingSpinner>(p => p
            .Add(c => c.Size, 40).Add(c => c.CssClass, "mine")
            .AddUnmatched("data-bob-test", "x"));
        Assert.Equal("40", cut.Find("svg").GetAttribute("width"));
        Assert.Contains("mine", cut.Find("[data-bob-part='root']").ClassName);
        Assert.Equal("x", cut.Find("[data-bob-part='root']").GetAttribute("data-bob-test"));
    }

    [Fact]
    public void ReducedMotionRuleExists()
    {
        // The spin is stopped by the shared prefers-reduced-motion rule in base.css; see ContractTests for the file.
        Assert.Contains("prefers-reduced-motion", File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "BobsComponents", "wwwroot", "css", "base.css")));
    }
}

public class FullScreenLoaderTests : TestContext
{
    public FullScreenLoaderTests() => Services.AddBobComponents();

    [Fact]
    public void ShowsAndAnnouncesMessage()
    {
        var cut = RenderComponent<FullScreenLoader>(p => p.Add(c => c.Message, "Signing you in…"));
        Assert.Equal("status", cut.Find(".bob-fullscreen-loader").GetAttribute("role"));
        Assert.Equal("Signing you in…", cut.Find("[data-bob-part='text']").TextContent);
        // The nested spinner's hidden label would repeat it; it must be hidden visually.
        Assert.Contains("bob-visually-hidden", cut.Find(".bob-spinner__label").ClassName);
    }

    [Fact]
    public void EmptyMessage_IsAParameterError()
    {
        var ex = Assert.Throws<BobParameterException>(() => RenderComponent<FullScreenLoader>(p => p.Add(c => c.Message, "")));
        Assert.Equal("BOB-PARAM-001", ex.Code);
    }
}

public class AsyncButtonTests : TestContext
{
    public AsyncButtonTests() => Services.AddBobComponents();

    private IRenderedComponent<AsyncButton> Render(Action<ComponentParameterCollectionBuilder<AsyncButton>>? configure = null) =>
        RenderComponent<AsyncButton>(p =>
        {
            p.AddChildContent("Save");
            configure?.Invoke(p);
        });

    [Fact]
    public void Idle_ShowsContent_AndIsEnabled()
    {
        var cut = Render();
        var button = cut.Find("button");
        Assert.Equal("Save", button.TextContent.Trim());
        Assert.Equal("button", button.GetAttribute("type"));
        Assert.False(button.HasAttribute("disabled"));
        Assert.False(button.HasAttribute("aria-busy"));
        Assert.Empty(cut.FindAll(".bob-spinner"));
    }

    [Fact]
    public void Busy_DisablesAndSwapsContentForAnAnnouncedSpinner()
    {
        var cut = Render(p => p.Add(c => c.Busy, true).Add(c => c.BusyLabel, "Saving…"));
        var button = cut.Find("button");
        Assert.True(button.HasAttribute("disabled"));
        Assert.Equal("true", button.GetAttribute("aria-busy"));
        Assert.DoesNotContain("Save", button.TextContent.Replace("Saving…", ""));
        Assert.Equal("Saving…", cut.Find(".bob-spinner__label").TextContent);
        Assert.Equal("status", cut.Find(".bob-spinner").GetAttribute("role"));
    }

    [Fact]
    public void Click_InvokesOnClick()
    {
        var clicks = 0;
        var cut = Render(p => p.Add(c => c.OnClick, EventCallback.Factory.Create(this, () => clicks++)));
        cut.Find("button").Click();
        Assert.Equal(1, clicks);
    }

    [Fact]
    public void Disabled_IsHonored()
    {
        var cut = Render(p => p.Add(c => c.Disabled, true));
        Assert.True(cut.Find("button").HasAttribute("disabled"));
    }

    [Fact]
    public void Variant_AddsBobButtonClasses_AndNoneByDefault()
    {
        Assert.DoesNotContain("bob-button", Render().Find("button").ClassName.Replace("bob-async-button", ""));
        var cut = Render(p => p.Add(c => c.Variant, BobButtonVariant.Danger).Add(c => c.CssClass, "mine"));
        var cls = cut.Find("button").ClassList;
        Assert.Contains("bob-button", cls);
        Assert.Contains("bob-button--danger", cls);
        Assert.Contains("mine", cls);
    }

    [Fact]
    public void ExtraAttributesFlowToTheButton()
    {
        var cut = Render(p => p.AddUnmatched("aria-label", "Save changes").AddUnmatched("data-bob-test", "1"));
        Assert.Equal("Save changes", cut.Find("button").GetAttribute("aria-label"));
    }

    [Fact]
    public void InvalidType_IsAParameterError_NamingTheValue()
    {
        var ex = Assert.Throws<BobParameterException>(() => Render(p => p.Add(c => c.Type, "link")));
        Assert.Equal("AsyncButton: parameter 'Type' was \"link\". Expected \"button\", \"submit\" or \"reset\".", ex.Message);
    }

    [Fact]
    public void StopPropagation_KeepsClickFromReachingParent()
    {
        var parentClicks = 0;
        var cut = RenderComponent<Wrapper>(p => p.Add(c => c.OnParentClick, () => parentClicks++));
        cut.Find("button").Click();
        Assert.Equal(0, parentClicks);
    }

    private sealed class Wrapper : ComponentBase
    {
        [Parameter] public Action? OnParentClick { get; set; }
        protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder b)
        {
            b.OpenElement(0, "div");
            b.AddAttribute(1, "onclick", EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, () => OnParentClick?.Invoke()));
            b.OpenComponent<AsyncButton>(2);
            b.AddAttribute(3, nameof(AsyncButton.StopPropagation), true);
            b.AddAttribute(4, nameof(AsyncButton.ChildContent), (RenderFragment)(c => c.AddContent(0, "x")));
            b.CloseComponent();
            b.CloseElement();
        }
    }
}

public class ModalSheetTests : TestContext
{
    public ModalSheetTests()
    {
        Services.AddBobComponents();
        JSInterop.Mode = JSRuntimeMode.Strict;
    }

    private (BunitJSModuleInterop focus, BunitJSModuleInterop history) SetupModules()
    {
        var focus = JSInterop.SetupModule(BobJs.FocusTrap);
        focus.SetupVoid("trap", _ => true).SetVoidResult();
        focus.SetupVoid("release").SetVoidResult();
        var history = JSInterop.SetupModule(BobJs.ModalHistory);
        history.SetupVoid("openModal", _ => true).SetVoidResult();
        history.SetupVoid("closeModal").SetVoidResult();
        return (focus, history);
    }

    private IRenderedComponent<ModalSheet> Render(Action<ComponentParameterCollectionBuilder<ModalSheet>>? configure = null) =>
        RenderComponent<ModalSheet>(p =>
        {
            p.Add(c => c.Show, true).Add(c => c.Title, "Edit item").AddChildContent("<p id='inside'>hi</p>");
            configure?.Invoke(p);
        });

    [Fact]
    public void Closed_RendersNothing()
    {
        var cut = RenderComponent<ModalSheet>(p => p.Add(c => c.Show, false).Add(c => c.Title, "x"));
        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void Open_IsAModalDialogWithAccessibleName()
    {
        SetupModules();
        var cut = Render();
        var sheet = cut.Find("[role='dialog']");
        Assert.Equal("true", sheet.GetAttribute("aria-modal"));
        Assert.Equal("Edit item", sheet.GetAttribute("aria-label"));
        Assert.Equal("-1", sheet.GetAttribute("tabindex"));
        Assert.Equal("Edit item", cut.Find("[data-bob-part='title']").TextContent);
        Assert.Equal("Close", cut.Find("[data-bob-part='close']").GetAttribute("aria-label"));
        Assert.NotEmpty(cut.FindAll("#inside"));
    }

    [Fact]
    public void Open_LoadsBothModulesFromStaticAssetPaths_AndTrapsFocusOnTheSheet()
    {
        var (focus, history) = SetupModules();
        Render();
        Assert.Contains(JSInterop.Invocations, i => i.Identifier == "import" && (string?)i.Arguments[0] == "./_content/BobsComponents/js/focusTrap.js");
        Assert.Contains(JSInterop.Invocations, i => i.Identifier == "import" && (string?)i.Arguments[0] == "./_content/BobsComponents/js/modalHistory.js");
        Assert.Single(focus.Invocations["trap"]);
        Assert.Single(history.Invocations["openModal"]);
    }

    [Fact]
    public void Closing_ReleasesTrap_AndPopsHistory()
    {
        var (focus, history) = SetupModules();
        var cut = Render();
        cut.SetParametersAndRender(p => p.Add(c => c.Show, false));
        cut.WaitForAssertion(() => Assert.Single(focus.Invocations["release"]));
        Assert.Single(history.Invocations["closeModal"]);
    }

    [Fact]
    public void BackButtonDisabled_NeverTouchesHistory()
    {
        SetupModules();
        Render(p => p.Add(c => c.EnableBackButton, false));
        Assert.DoesNotContain(JSInterop.Invocations, i => i.Arguments.Any(a => a?.ToString()?.Contains("modalHistory") == true));
    }

    [Fact]
    public void Escape_BackdropAndCloseButton_AllCallOnClose_ButOtherKeysDoNot()
    {
        SetupModules();
        var closes = 0;
        var cut = Render(p => p.Add(c => c.OnClose, EventCallback.Factory.Create(this, () => closes++)));

        cut.Find("[role='dialog']").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });
        Assert.Equal(1, closes);
        cut.Find("[role='dialog']").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "a" });
        Assert.Equal(1, closes);

        cut.Find("[data-bob-part='backdrop']").Click();
        Assert.Equal(2, closes);

        cut.Find("[data-bob-part='close']").Click();
        Assert.Equal(3, closes);
    }

    [Fact]
    public async Task BrowserBackButton_CallsOnClose_WithoutPoppingHistoryAgain()
    {
        var (_, history) = SetupModules();
        var closes = 0;
        var cut = Render(p => p.Add(c => c.OnClose, EventCallback.Factory.Create(this, () => closes++)));

        await cut.InvokeAsync(() => cut.Instance.OnModalBackButton());
        Assert.Equal(1, closes);

        cut.SetParametersAndRender(p => p.Add(c => c.Show, false));
        Assert.Empty(history.Invocations["closeModal"]);
    }

    [Fact]
    public void CustomHeader_NeedsAnAriaLabel()
    {
        SetupModules();
        var ex = Assert.Throws<BobParameterException>(() => RenderComponent<ModalSheet>(p => p
            .Add(c => c.Show, true).Add(c => c.HeaderContent, "<b>custom</b>")));
        Assert.Equal("BOB-PARAM-002", ex.Code);

        var cut = RenderComponent<ModalSheet>(p => p
            .Add(c => c.Show, true).Add(c => c.HeaderContent, "<b>custom</b>").Add(c => c.AriaLabel, "Custom dialog"));
        Assert.Equal("Custom dialog", cut.Find("[role='dialog']").GetAttribute("aria-label"));
        Assert.Empty(cut.FindAll("[data-bob-part='close']"));
    }

    [Fact]
    public void MissingFocusTrapScript_SurfacesAsMissingAssetNamingTheScript()
    {
        Services.AddSingleton<IJSRuntime>(new MissingScriptJs());

        var cut = RenderComponent<BobErrorBoundary>(p => p.AddChildContent<ModalSheet>(m => m
            .Add(c => c.Show, true).Add(c => c.Title, "x").Add(c => c.EnableBackButton, false)));

        cut.WaitForAssertion(() =>
        {
            Assert.Equal("BOB-ASSET-001", cut.Find(".bob-error-card").GetAttribute("data-bob-code"));
            Assert.Contains("./_content/BobsComponents/js/focusTrap.js", cut.Find("[data-bob-part='message']").TextContent);
        });
    }

    [Fact]
    public void StackedSheets_EachGetTheirOwnTrap()
    {
        var (focus, _) = SetupModules();
        Render();
        Render();
        Assert.Equal(2, focus.Invocations["trap"].Count);
    }
}

internal sealed class MissingScriptJs : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
        throw new JSException("Failed to fetch dynamically imported module");

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
        InvokeAsync<TValue>(identifier, args);
}

public class ConfirmDialogTests : TestContext
{
    public ConfirmDialogTests()
    {
        Services.AddBobComponents();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Hidden_RendersNothing()
    {
        var cut = RenderComponent<ConfirmDialog>();
        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void ShowsTitleMessageAndLabels_WithoutBootstrapClasses()
    {
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(c => c.Show, true).Add(c => c.Title, "Delete workout?").Add(c => c.Message, "This cannot be undone.")
            .Add(c => c.ConfirmLabel, "Remove").Add(c => c.CancelLabel, "Keep"));
        Assert.Equal("Delete workout?", cut.Find("[role='dialog']").GetAttribute("aria-label"));
        Assert.Equal("This cannot be undone.", cut.Find("[data-bob-part='message']").TextContent);
        Assert.Equal("Keep", cut.Find("[data-bob-part='cancel']").TextContent.Trim());
        Assert.Equal("Remove", cut.Find(".bob-confirm__confirm").TextContent.Trim());
        Assert.DoesNotContain("btn", cut.Markup.Replace("bob-button", ""));
        Assert.Contains("bob-button--danger", cut.Find(".bob-confirm__confirm").ClassList);
    }

    [Fact]
    public void ConfirmAndCancel_CallTheirCallbacks()
    {
        int confirms = 0, cancels = 0;
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(c => c.Show, true)
            .Add(c => c.OnConfirm, EventCallback.Factory.Create(this, () => confirms++))
            .Add(c => c.OnCancel, EventCallback.Factory.Create(this, () => cancels++)));

        cut.Find(".bob-confirm__confirm").Click();
        cut.Find("[data-bob-part='cancel']").Click();
        cut.Find("[role='dialog']").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });
        cut.Find("[data-bob-part='backdrop']").Click();

        Assert.Equal(1, confirms);
        Assert.Equal(3, cancels);
    }

    [Fact]
    public void Busy_DisablesConfirm_AndAnnouncesItsAction()
    {
        var cut = RenderComponent<ConfirmDialog>(p => p
            .Add(c => c.Show, true).Add(c => c.Busy, true).Add(c => c.ConfirmLabel, "Delete"));
        Assert.True(cut.Find(".bob-confirm__confirm").HasAttribute("disabled"));
        Assert.Equal("Delete…", cut.Find(".bob-confirm__confirm .bob-spinner__label").TextContent);
    }

    [Fact]
    public void Danger_False_UsesPrimaryLook()
    {
        var cut = RenderComponent<ConfirmDialog>(p => p.Add(c => c.Show, true).Add(c => c.Danger, false));
        Assert.Contains("bob-button--primary", cut.Find(".bob-confirm__confirm").ClassList);
    }

    [Fact]
    public void ThrowingConfirmHandler_BecomesACallbackError()
    {
        var cut = RenderComponent<BobErrorBoundary>(p => p.AddChildContent<ConfirmDialog>(c => c
            .Add(d => d.Show, true)
            .Add(d => d.OnConfirm, EventCallback.Factory.Create(this, () => throw new InvalidOperationException("delete failed")))));

        cut.Find(".bob-confirm__confirm").Click();

        Assert.Equal("BOB-CB-001", cut.Find(".bob-error-card").GetAttribute("data-bob-code"));
        Assert.Equal("delete failed", cut.Find("[data-bob-part='message']").TextContent);
        Assert.Contains("OnConfirm", cut.Find("[data-bob-part='facts']").TextContent);
    }
}

public class ToastTests : TestContext
{
    public ToastTests() => Services.AddBobComponents(o => o.Toasts.DefaultDuration = TimeSpan.FromMilliseconds(150));

    [Fact]
    public void Host_IsAPoliteLiveRegion()
    {
        var cut = RenderComponent<ToastHost>();
        Assert.Equal("polite", cut.Find("[data-bob-part='host']").GetAttribute("aria-live"));
    }

    [Fact]
    public void Show_AddsToast_ThenAutoDismisses()
    {
        var cut = RenderComponent<ToastHost>();
        var service = Services.GetRequiredService<IBobToastService>();

        service.Show("Saved", BobToastLevel.Success);

        cut.WaitForAssertion(() => Assert.Equal("Saved", cut.Find("[data-bob-part='text']").TextContent));
        Assert.Contains("bob-toast--success", cut.Find(".bob-toast").ClassList);
        cut.WaitForAssertion(() => Assert.Empty(cut.FindAll(".bob-toast")), TimeSpan.FromSeconds(3));
    }

    [Fact]
    public void ErrorToast_IsAnAlert_OthersAreNot_AndIconsAreDecorative()
    {
        var cut = RenderComponent<ToastHost>();
        var service = Services.GetRequiredService<IBobToastService>();
        service.Show("bad", BobToastLevel.Error, TimeSpan.FromSeconds(30));
        service.Show("fyi", BobToastLevel.Info, TimeSpan.FromSeconds(30));

        cut.WaitForState(() => cut.FindAll(".bob-toast").Count == 2);
        Assert.Equal("alert", cut.Find(".bob-toast--error").GetAttribute("role"));
        Assert.Null(cut.Find(".bob-toast--info").GetAttribute("role"));
        Assert.All(cut.FindAll(".bob-toast__icon"), i => Assert.Equal("true", i.GetAttribute("aria-hidden")));
    }

    [Fact]
    public async Task ExplicitDuration_Wins_AndMaxVisibleDropsTheOldest()
    {
        var cut = RenderComponent<ToastHost>();
        var service = Services.GetRequiredService<IBobToastService>();
        service.Show("long", BobToastLevel.Info, TimeSpan.FromSeconds(30));
        await Task.Delay(500); // default is 150 ms; the explicit duration must outlive it
        cut.WaitForAssertion(() => Assert.Single(cut.FindAll(".bob-toast")));

        for (var i = 0; i < 7; i++) service.Show($"t{i}", BobToastLevel.Info, TimeSpan.FromSeconds(30));
        cut.WaitForAssertion(() => Assert.Equal(5, cut.FindAll(".bob-toast").Count));
        Assert.Equal("t6", cut.FindAll("[data-bob-part='text']").Last().TextContent);
        Assert.DoesNotContain(cut.FindAll("[data-bob-part='text']"), t => t.TextContent == "long");
    }

    [Fact]
    public void Unsubscribes_OnDispose()
    {
        var service = Services.GetRequiredService<IBobToastService>();
        var cut = RenderComponent<ToastHost>();
        DisposeComponents();
        service.Show("late"); // must not throw
        Assert.NotNull(cut);
    }

    [Fact]
    public void WithoutRegistration_ToastHostReportsTheMissingService()
    {
        using var ctx = new TestContext();
        var ex = Assert.Throws<BobMissingDependencyException>(() => ctx.RenderComponent<ToastHost>());
        Assert.Equal("BOB-ASSET-002", ex.Code);
        Assert.Contains(typeof(IBobToastService).FullName!, ex.Message);
    }
}
