using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using Microsoft.JSInterop;

namespace BobsComponents.Errors;

/// <summary>Turns any exception into a <see cref="BobError"/>.</summary>
public interface IBobErrorClassifier
{
    /// <param name="exception">The exception to classify.</param>
    /// <param name="component">Name of the component that caught or raised it, if known.</param>
    BobError Classify(Exception exception, string? component = null);
}

/// <summary>
/// Default classifier. Rules run in order and look only at exception types, structured properties and the
/// assemblies of stack frames. They never read message text.
/// <list type="number">
/// <item>Unwrap <see cref="AggregateException"/> and <see cref="TargetInvocationException"/>.</item>
/// <item><see cref="BobException"/>: use its own origin and code.</item>
/// <item><see cref="JsonException"/>: data shape, with the JSON path.</item>
/// <item><see cref="JSException"/>: missing asset.</item>
/// <item><see cref="HttpRequestException"/> and timeouts: network failure.</item>
/// <item>Anything thrown first from app code (not BobsComponents, not the framework): your callback threw.</item>
/// <item><see cref="ArgumentException"/> from library code: wrong parameters.</item>
/// <item>Everything else: component bug.</item>
/// </list>
/// </summary>
public sealed class BobErrorClassifier : IBobErrorClassifier
{
    public const string ComponentBugCode = "BOB-COMP-001";
    public const string TransportCode = "BOB-NET-002";
    public const string TimeoutCode = "BOB-NET-003";

    public BobError Classify(Exception exception, string? component = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        var root = Unwrap(exception);
        var (origin, code, message, facts, status) = Decide(root, component);

        return new BobError
        {
            Origin = origin,
            Code = code,
            Message = message,
            Component = (root as BobException)?.Component ?? component,
            Facts = facts,
            StatusCode = status,
            StackTrace = (root is BobCallbackException { InnerException: { } cb } ? cb : root).StackTrace,
            Inner = Chain(root),
            Exception = root,
        };
    }

    private static Exception Unwrap(Exception ex)
    {
        while (true)
        {
            if (ex is AggregateException { InnerExceptions.Count: 1 } agg) ex = agg.InnerExceptions[0];
            else if (ex is TargetInvocationException { InnerException: { } inner }) ex = inner;
            else return ex;
        }
    }

    private static (BobErrorOrigin, string, string, Dictionary<string, string>, int?) Decide(Exception ex, string? component)
    {
        var facts = new Dictionary<string, string>();

        switch (ex)
        {
            case BobException bob:
                foreach (var kv in bob.Facts) facts[kv.Key] = kv.Value;
                return (bob.Origin, bob.Code, bob.Message, facts, (bob as BobHttpException)?.StatusCode);

            case JsonException json:
                facts["path"] = json.Path ?? "$";
                facts["expected"] = "valid JSON matching the target type";
                facts["actual"] = "JSON that failed to deserialize";
                return (BobErrorOrigin.DataShapeMismatch, BobDataShapeException.DeserializeCode, json.Message, facts, null);

            case JSException js:
                facts["script"] = "(identifier not captured; wrap the call with BobInterop to record it)";
                return (BobErrorOrigin.MissingAssetOrService, BobMissingDependencyException.ScriptCode, js.Message, facts, null);

            case HttpRequestException http:
                if (http.StatusCode is { } sc)
                {
                    facts["status"] = $"{(int)sc} {sc}";
                    return (BobErrorOrigin.NetworkFailure, BobHttpException.StatusErrorCode, http.Message, facts, (int)sc);
                }
                return (BobErrorOrigin.NetworkFailure, TransportCode, http.Message, facts, null);

            case TaskCanceledException { InnerException: TimeoutException } or TimeoutException:
                return (BobErrorOrigin.NetworkFailure, TimeoutCode, ex.Message, facts, null);
        }

        if (!BobStack.ThrownByLibrary(ex))
        {
            facts["callback"] = "(unnamed)";
            var frame = BobStack.TopAppFrame(ex);
            if (frame is not null) facts["yourFrame"] = frame;
            return (BobErrorOrigin.CallbackThrew, BobCallbackException.CallbackCode, ex.Message, facts, null);
        }

        if (ex is ArgumentException arg)
        {
            facts["parameter"] = arg.ParamName ?? "(unknown)";
            return (BobErrorOrigin.WrongParameters, BobParameterException.InvalidCode, ex.Message, facts, null);
        }

        facts["exceptionType"] = ex.GetType().FullName ?? ex.GetType().Name;
        return (BobErrorOrigin.ComponentBug, ComponentBugCode, ex.Message, facts, null);
    }

    private static List<BobErrorChainItem> Chain(Exception root)
    {
        var list = new List<BobErrorChainItem>();
        for (var e = root.InnerException; e is not null; e = e.InnerException)
        {
            list.Add(new BobErrorChainItem(e.GetType().FullName ?? e.GetType().Name, e.Message, e.StackTrace));
        }
        return list;
    }
}
