using System.Diagnostics;
using BobsComponents.Errors;
using BobsComponents.Infrastructure;
using BobsComponents.Options;

namespace BobsComponents.DevKit;

/// <summary>
/// DelegatingHandler that records each request for the dev kit Network panel and reports failures to the
/// error hub with the server's own message. Headers are never recorded, so tokens cannot leak into the log.
/// Does nothing when the dev kit is off.
/// </summary>
public sealed class BobNetworkHandler : DelegatingHandler
{
    private readonly BobNetworkLog _log;
    private readonly IBobErrorHub? _hub;
    private readonly BobOptions _options;

    public BobNetworkHandler(BobNetworkLog log, BobOptions options, IBobErrorHub? hub = null)
    {
        _log = log;
        _options = options;
        _hub = hub;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (!BobDevKitState.IsEnabled(_options))
            return await base.SendAsync(request, ct);

        var started = DateTimeOffset.UtcNow;
        var watch = Stopwatch.StartNew();
        var dev = _options.DevKit;
        var url = request.RequestUri?.ToString() ?? "?";
        string? requestBody = null;
        if (dev.CaptureRequestBodies && request.Content is not null)
            requestBody = Truncate(await request.Content.ReadAsStringAsync(ct), dev.MaxBodyChars);

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException || ex.InnerException is TimeoutException)
        {
            watch.Stop();
            _log.Add(new BobNetworkEntry(started, request.Method.Method, url, null, null, watch.Elapsed, requestBody, null, ex.Message));
            _hub?.Report(ex, "HttpClient");
            throw;
        }

        watch.Stop();
        string? responseBody = null;
        if (dev.CaptureResponseBodies && response.Content is not null)
        {
            // Buffer so the app can still read the body after we did.
            await response.Content.LoadIntoBufferAsync(ct);
            responseBody = Truncate(await response.Content.ReadAsStringAsync(ct), dev.MaxBodyChars);
        }

        _log.Add(new BobNetworkEntry(started, request.Method.Method, url, (int)response.StatusCode, response.ReasonPhrase,
            watch.Elapsed, requestBody, responseBody, null));

        if (!response.IsSuccessStatusCode)
        {
            var message = BobHttp.ExtractServerMessage(responseBody ?? "");
            if (string.IsNullOrWhiteSpace(message)) message = response.ReasonPhrase ?? $"HTTP {(int)response.StatusCode}";
            _hub?.Report(new BobHttpException("HttpClient", request.Method.Method, url,
                (int)response.StatusCode, response.ReasonPhrase, message), "HttpClient");
        }
        return response;
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max] + $"… (+{value.Length - max} more characters)";
}
