using System.Net.Http.Json;
using System.Text.Json;
using BobsComponents.Errors;

namespace BobsComponents.Infrastructure;

/// <summary>Helpers that keep the server's own words when a call fails.</summary>
public static class BobHttp
{
    /// <summary>
    /// Throws <see cref="BobHttpException"/> for a non-success response. The message is the server's own:
    /// the <c>detail</c> (else <c>title</c>) of a problem details body, otherwise the raw body text, otherwise the reason phrase.
    /// </summary>
    public static async Task EnsureSuccessAsync(HttpResponseMessage response, string? component = null, CancellationToken ct = default)
    {
        if (response.IsSuccessStatusCode) return;

        var body = response.Content is null ? "" : await response.Content.ReadAsStringAsync(ct);
        var message = ExtractServerMessage(body);
        if (string.IsNullOrWhiteSpace(message)) message = response.ReasonPhrase ?? $"HTTP {(int)response.StatusCode}";

        var request = response.RequestMessage;
        throw new BobHttpException(component, request?.Method.Method ?? "?", request?.RequestUri?.ToString() ?? "?",
            (int)response.StatusCode, response.ReasonPhrase, message);
    }

    /// <summary>Reads JSON and turns a deserialization failure into a data-shape error naming the JSON path.</summary>
    public static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, string? component = null, CancellationToken ct = default)
    {
        await EnsureSuccessAsync(response, component, ct);
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        }
        catch (JsonException ex)
        {
            throw new BobDataShapeException(component, ex.Path ?? "$", typeof(T).Name, "JSON that failed to deserialize", ex);
        }
    }

    internal static string ExtractServerMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return "";
        var trimmed = body.TrimStart();
        if (trimmed.StartsWith('{'))
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                foreach (var name in new[] { "detail", "title", "message", "error" })
                {
                    if (root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String
                        && !string.IsNullOrWhiteSpace(prop.GetString()))
                        return prop.GetString()!;
                }
            }
            catch (JsonException)
            {
                // Not JSON after all: fall through to the raw body.
            }
        }
        return body.Trim();
    }
}
