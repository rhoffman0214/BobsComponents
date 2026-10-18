using System.Diagnostics.CodeAnalysis;
using BobsComponents.Errors;
using BobsComponents.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BobsComponents.Infrastructure;

/// <summary>
/// Base class for BobsComponents components. Gives every component the same three safety nets:
/// parameter validation that raises <see cref="BobParameterException"/>, callback invocation that raises
/// <see cref="BobCallbackException"/>, and a way to send fire-and-forget failures to the error hub.
/// </summary>
public abstract class BobComponentBase : ComponentBase
{
    [Inject] protected IServiceProvider BobServices { get; set; } = default!;

    /// <summary>Name used in error messages. Defaults to the component's type name.</summary>
    protected virtual string ComponentName => GetType().Name;

    protected BobOptions BobOptions => BobServices.GetService<BobOptions>() ?? s_defaultOptions;
    private static readonly BobOptions s_defaultOptions = new();

    /// <summary>Gets a service or throws <see cref="BobMissingDependencyException"/> naming it.</summary>
    protected T RequireService<T>() where T : class =>
        BobServices.GetService<T>() ?? throw BobMissingDependencyException.Service(ComponentName, typeof(T));

    protected T? OptionalService<T>() where T : class => BobServices.GetService<T>();

    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // Set and validate first, so a bad parameter is reported as such instead of surfacing as a
        // NullReferenceException from the first render. Then run the normal lifecycle with no new parameters.
        parameters.SetParameterProperties(this);
        ValidateParameters(new BobParameterValidator(ComponentName));
        await base.SetParametersAsync(ParameterView.Empty);
    }

    /// <summary>Override to check parameters. Throws on the first problem found.</summary>
    protected virtual void ValidateParameters(BobParameterValidator check) { }

    /// <summary>Invoke an app-supplied callback; anything it throws is wrapped as a callback error.</summary>
    protected Task InvokeCallbackAsync(string name, EventCallback callback) =>
        BobCallbacks.InvokeAsync(ComponentName, name, callback);

    /// <summary>Invoke an app-supplied callback; anything it throws is wrapped as a callback error.</summary>
    protected Task InvokeCallbackAsync<T>(string name, EventCallback<T> callback, T value) =>
        BobCallbacks.InvokeAsync(ComponentName, name, callback, value);

    /// <summary>
    /// Send a failure from fire-and-forget work to the error hub. An exception inside an un-awaited task never
    /// reaches an error boundary, so this is the only place it can be seen.
    /// </summary>
    protected void ReportError(Exception exception) =>
        BobServices.GetService<IBobErrorHub>()?.Report(exception, ComponentName);
}

/// <summary>Parameter checks that throw <see cref="BobParameterException"/> with the component name filled in.</summary>
public readonly struct BobParameterValidator
{
    private readonly string _component;
    public BobParameterValidator(string component) => _component = component;

    public void Required(string parameter, object? value, string expected)
    {
        if (value is null || value is string { Length: 0 })
            throw BobParameterException.Missing(_component, parameter, expected);
    }

    public void Range(string parameter, int value, int min, int max)
    {
        if (value < min || value > max)
            throw BobParameterException.Invalid(_component, parameter, value, $"a value from {min} to {max}");
    }

    public void Defined<TEnum>(string parameter, TEnum value) where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
            throw BobParameterException.Invalid(_component, parameter, value, $"a defined {typeof(TEnum).Name} value");
    }

    public void Must(string parameter, bool valid, object? passed, string expected)
    {
        if (!valid) throw BobParameterException.Invalid(_component, parameter, passed, expected);
    }

    public void NotBoth(string parameter, bool a, string other, bool b, string expected)
    {
        if (a && b) throw BobParameterException.Conflict(_component, parameter, other, expected);
    }
}

/// <summary>Helpers that put app code behind the callback error origin.</summary>
public static class BobCallbacks
{
    public static async Task InvokeAsync(string? component, string name, EventCallback callback)
    {
        if (!callback.HasDelegate) return;
        try
        {
            await callback.InvokeAsync();
        }
        catch (Exception ex) when (ex is not BobException)
        {
            throw new BobCallbackException(component, name, ex);
        }
    }

    public static async Task InvokeAsync<T>(string? component, string name, EventCallback<T> callback, T value)
    {
        if (!callback.HasDelegate) return;
        try
        {
            await callback.InvokeAsync(value);
        }
        catch (Exception ex) when (ex is not BobException)
        {
            throw new BobCallbackException(component, name, ex);
        }
    }
}
