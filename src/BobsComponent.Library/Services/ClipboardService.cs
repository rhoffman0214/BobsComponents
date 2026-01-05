using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BobsComponent.Library.Services;

/// <summary>
/// Service for copying text to the system clipboard
/// </summary>
public class ClipboardService : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    public ClipboardService(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/BobsComponent.Library/js/clipboard.js").AsTask());
    }

    /// <summary>
    /// Copies the specified text to the clipboard
    /// </summary>
    /// <param name="text">The text to copy</param>
    /// <returns>True if successful, false otherwise</returns>
    public async Task<bool> CopyToClipboardAsync(string text)
    {
        var module = await _moduleTask.Value;
        return await module.InvokeAsync<bool>("copyToClipboard", text);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
