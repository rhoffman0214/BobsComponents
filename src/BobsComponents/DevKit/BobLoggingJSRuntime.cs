using System.Diagnostics;
using System.Text.Json;
using BobsComponents.Errors;
using Microsoft.JSInterop;

namespace BobsComponents.DevKit;

/// <summary>
/// IJSRuntime decorator for the dev kit Interop panel: records each call, and reports failures (a missing
/// module, a missing function) to the error hub as missing-asset errors that name the script.
/// </summary>
public sealed class BobLoggingJSRuntime : IJSRuntime
{
    private readonly IJSRuntime _inner;
    private readonly BobInteropLog _log;
    private readonly IBobErrorHub? _hub;

    public BobLoggingJSRuntime(IJSRuntime inner, BobInteropLog log, IBobErrorHub? hub)
    {
        _inner = inner;
        _log = log;
        _hub = hub;
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
        InvokeCoreAsync<TValue>("window", identifier, args, () => _inner.InvokeAsync<TValue>(identifier, args));

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
        InvokeCoreAsync<TValue>("window", identifier, args, () => _inner.InvokeAsync<TValue>(identifier, cancellationToken, args));

    internal async ValueTask<TValue> InvokeCoreAsync<TValue>(string target, string identifier, object?[]? args, Func<ValueTask<TValue>> call)
    {
        var started = DateTimeOffset.UtcNow;
        var watch = Stopwatch.StartNew();
        try
        {
            var result = await call();
            _log.Add(new BobInteropEntry(started, target, identifier, Describe(args), watch.Elapsed, null));

            if (result is IJSObjectReference module && identifier == "import")
                return (TValue)(object)new LoggingModule(module, this, args is { Length: > 0 } ? args[0]?.ToString() ?? "module" : "module");
            return result;
        }
        catch (JSException ex)
        {
            _log.Add(new BobInteropEntry(started, target, identifier, Describe(args), watch.Elapsed, ex.Message));
            var script = identifier == "import" && args is { Length: > 0 } ? args[0]?.ToString() ?? identifier : $"{target}.{identifier}";
            _hub?.Report(BobMissingDependencyException.Script(null, script, ex), "IJSRuntime");
            throw;
        }
    }

    private static string Describe(object?[]? args)
    {
        if (args is null || args.Length == 0) return "";
        try
        {
            return string.Join(", ", args.Select(a => a switch
            {
                null => "null",
                _ => Shorten(JsonSerializer.Serialize(a)),
            }));
        }
        catch (Exception)
        {
            return $"({args.Length} args, not serializable for display)";
        }
    }

    private static string Shorten(string s) => s.Length <= 200 ? s : s[..200] + "…";

    private sealed class LoggingModule : IJSObjectReference
    {
        private readonly IJSObjectReference _module;
        private readonly BobLoggingJSRuntime _owner;
        private readonly string _name;

        public LoggingModule(IJSObjectReference module, BobLoggingJSRuntime owner, string name)
        {
            _module = module;
            _owner = owner;
            _name = name;
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            _owner.InvokeCoreAsync<TValue>(_name, identifier, args, () => _module.InvokeAsync<TValue>(identifier, args));

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            _owner.InvokeCoreAsync<TValue>(_name, identifier, args, () => _module.InvokeAsync<TValue>(identifier, cancellationToken, args));

        public ValueTask DisposeAsync() => _module.DisposeAsync();
    }
}
