using BobsComponents.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace BobsComponents.Docs;

/// <summary>Registers the docs site services. Call once in Program.cs next to <c>AddBobComponents</c>.</summary>
public static class BobDocsServiceCollectionExtensions
{
    public static IServiceCollection AddBobDocs(this IServiceCollection services)
    {
        services.AddScoped<DocsJs>();
        return services;
    }
}

/// <summary>Wraps bobdocs.js. The module loads on first use through <see cref="BobJs"/>, so a missing script names its path.</summary>
public sealed class DocsJs(IJSRuntime js) : IAsyncDisposable
{
    public const string ScriptPath = "./_content/BobsComponents.Docs/js/bobdocs.js";

    private IJSObjectReference? _module;

    private async ValueTask<IJSObjectReference> ModuleAsync() =>
        _module ??= await BobJs.ImportAsync(js, "BobDocs", ScriptPath);

    public async ValueTask ObserveFrameAsync<T>(string key, ElementReference element, DotNetObjectReference<T> target) where T : class =>
        await BobJs.CallAsync(await ModuleAsync(), "BobDocs", ScriptPath, "observeFrame", key, element, target);

    public async ValueTask DisposeFrameAsync(string key)
    {
        if (_module is null) return;
        try { await _module.InvokeVoidAsync("disposeFrame", key); } catch (JSDisconnectedException) { }
    }

    public async ValueTask SetInspectorAsync(string key, bool enabled, string indexJson) =>
        await BobJs.CallAsync(await ModuleAsync(), "BobDocs", ScriptPath, "setInspectorEnabled", key, enabled, indexJson);

    public async ValueTask HighlightAsync(string key, int index) =>
        await BobJs.CallAsync(await ModuleAsync(), "BobDocs", ScriptPath, "highlightElement", key, index);

    public async ValueTask ClearHighlightAsync()
    {
        if (_module is null) return;
        try { await _module.InvokeVoidAsync("clearHighlight"); } catch (JSDisconnectedException) { }
    }

    public async ValueTask<bool> CopyAsync(string text) =>
        await (await ModuleAsync()).InvokeAsync<bool>("copyText", text);

    public async ValueTask ScrollToIdAsync(string id) =>
        await (await ModuleAsync()).InvokeVoidAsync("scrollToId", id);

    public async ValueTask DisposeAsync()
    {
        if (_module is null) return;
        try { await _module.DisposeAsync(); } catch (JSDisconnectedException) { }
    }
}
