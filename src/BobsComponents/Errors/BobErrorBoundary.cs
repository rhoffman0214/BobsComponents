using BobsComponents.Components;
using BobsComponents.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace BobsComponents.Errors;

/// <summary>
/// Error boundary that classifies whatever its children throw into a <see cref="BobError"/>, publishes it to
/// the <see cref="IBobErrorHub"/> (so the dev kit sees it) and shows a <see cref="BobErrorCard"/>.
/// Works without <c>AddBobComponents()</c>; it then uses default options and a private classifier.
/// </summary>
public sealed class BobErrorBoundary : ErrorBoundaryBase
{
    [Inject] private IServiceProvider Services { get; set; } = default!;

    private BobError? _error;

    /// <summary>Name shown on the card when the thrower is unknown, e.g. the page this boundary wraps.</summary>
    [Parameter] public string? ComponentName { get; set; }

    /// <summary>Replace the default card. Receives the classified error.</summary>
    [Parameter] public RenderFragment<BobError>? ErrorTemplate { get; set; }

    /// <summary>Called after an error is classified. An exception thrown here is ignored.</summary>
    [Parameter] public EventCallback<BobError> OnBobError { get; set; }

    /// <summary>The last classified error, or null while healthy.</summary>
    public BobError? Error => _error;

    protected override async Task OnErrorAsync(Exception exception)
    {
        var hub = Services.GetService<IBobErrorHub>();
        var classifier = Services.GetService<IBobErrorClassifier>() ?? s_fallbackClassifier;
        _error = hub is not null ? hub.Report(exception, ComponentName) : classifier.Classify(exception, ComponentName);

        if (OnBobError.HasDelegate)
        {
            try { await OnBobError.InvokeAsync(_error); }
            catch { /* a failing error handler must not hide the original error */ }
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (CurrentException is null)
        {
            builder.AddContent(0, ChildContent);
            return;
        }

        _error ??= (Services.GetService<IBobErrorClassifier>() ?? s_fallbackClassifier).Classify(CurrentException, ComponentName);

        if (ErrorTemplate is not null)
        {
            builder.AddContent(1, ErrorTemplate(_error));
            return;
        }

        builder.OpenComponent<BobErrorCard>(2);
        builder.AddAttribute(3, nameof(BobErrorCard.Error), _error);
        builder.AddAttribute(4, nameof(BobErrorCard.OnRecover), EventCallback.Factory.Create(this, Recover));
        builder.CloseComponent();
    }

    private static readonly BobErrorClassifier s_fallbackClassifier = new();
}
