using BobsComponents.Errors;
using Microsoft.JSInterop;

namespace BobsComponents.Infrastructure;

/// <summary>
/// Loads the package's JavaScript modules from their static web asset paths and turns failures into
/// <see cref="BobMissingDependencyException"/> that names the script.
/// </summary>
public static class BobJs
{
    /// <summary>Static web asset base for this package, relative to the document base.</summary>
    public const string AssetBase = "./_content/BobsComponents/";

    public const string FocusTrap = AssetBase + "js/focusTrap.js";
    public const string ModalHistory = AssetBase + "js/modalHistory.js";
    public const string DevKit = AssetBase + "js/devkit.js";

    public static async ValueTask<IJSObjectReference> ImportAsync(IJSRuntime js, string component, string path)
    {
        try
        {
            return await js.InvokeAsync<IJSObjectReference>("import", path);
        }
        catch (JSException ex)
        {
            throw BobMissingDependencyException.Script(component, path, ex);
        }
    }

    public static async ValueTask CallAsync(IJSObjectReference module, string component, string script, string function, params object?[] args)
    {
        try
        {
            await module.InvokeVoidAsync(function, args);
        }
        catch (JSException ex)
        {
            throw BobMissingDependencyException.Script(component, $"{script}#{function}", ex);
        }
    }
}
