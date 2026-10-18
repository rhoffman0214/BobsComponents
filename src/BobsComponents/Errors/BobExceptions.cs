namespace BobsComponents.Errors;

/// <summary>
/// Base for exceptions that BobsComponents raises on purpose. They carry their own origin and code so the
/// classifier never has to inspect message text.
/// </summary>
public abstract class BobException : Exception
{
    protected BobException(BobErrorOrigin origin, string code, string message, string? component, Exception? inner = null)
        : base(message, inner)
    {
        Origin = origin;
        Code = code;
        Component = component;
    }

    public BobErrorOrigin Origin { get; }
    public string Code { get; }
    public string? Component { get; }

    /// <summary>Structured facts copied onto the <see cref="BobError"/>.</summary>
    public IDictionary<string, string> Facts { get; } = new Dictionary<string, string>();
}

/// <summary>A component parameter is missing, invalid, or conflicts with another.</summary>
public sealed class BobParameterException : BobException
{
    public const string MissingCode = "BOB-PARAM-001";
    public const string InvalidCode = "BOB-PARAM-002";
    public const string ConflictCode = "BOB-PARAM-003";

    private BobParameterException(string code, string message, string component, string parameter, string? passed, string expected)
        : base(BobErrorOrigin.WrongParameters, code, message, component)
    {
        Parameter = parameter;
        Facts["parameter"] = parameter;
        Facts["passed"] = passed ?? "(not set)";
        Facts["expected"] = expected;
    }

    public string Parameter { get; }

    public static BobParameterException Missing(string component, string parameter, string expected) =>
        new(MissingCode, $"{component}: required parameter '{parameter}' was not set. Expected {expected}.",
            component, parameter, null, expected);

    public static BobParameterException Invalid(string component, string parameter, object? passed, string expected) =>
        new(InvalidCode, $"{component}: parameter '{parameter}' was {Describe(passed)}. Expected {expected}.",
            component, parameter, Describe(passed), expected);

    public static BobParameterException Conflict(string component, string parameter, string otherParameter, string expected) =>
        new(ConflictCode, $"{component}: parameters '{parameter}' and '{otherParameter}' conflict. {expected}",
            component, parameter, null, expected);

    private static string Describe(object? value) => value switch
    {
        null => "null",
        string s => $"\"{s}\"",
        _ => value.ToString() ?? value.GetType().Name,
    };
}

/// <summary>Data failed a shape check or could not be deserialized; <see cref="Path"/> says where.</summary>
public sealed class BobDataShapeException : BobException
{
    public const string ShapeCode = "BOB-DATA-001";
    public const string DeserializeCode = "BOB-DATA-002";

    public BobDataShapeException(string? component, string path, string expectedType, string actual, Exception? inner = null)
        : base(BobErrorOrigin.DataShapeMismatch, inner is null ? ShapeCode : DeserializeCode,
            $"Data shape mismatch at '{path}': expected {expectedType}, got {actual}.", component, inner)
    {
        Path = path;
        Facts["path"] = path;
        Facts["expected"] = expectedType;
        Facts["actual"] = actual;
    }

    public string Path { get; }
}

/// <summary>A script, JS function or service a component needs is not there.</summary>
public sealed class BobMissingDependencyException : BobException
{
    public const string ScriptCode = "BOB-ASSET-001";
    public const string ServiceCode = "BOB-ASSET-002";

    private BobMissingDependencyException(string code, string message, string? component, Exception? inner)
        : base(BobErrorOrigin.MissingAssetOrService, code, message, component, inner) { }

    public static BobMissingDependencyException Script(string? component, string script, Exception? inner = null)
    {
        var e = new BobMissingDependencyException(ScriptCode,
            $"Script or JS function '{script}' could not be loaded or called." + (inner is null ? "" : $" {inner.Message}"),
            component, inner);
        e.Facts["script"] = script;
        return e;
    }

    public static BobMissingDependencyException Service(string? component, Type serviceType)
    {
        var e = new BobMissingDependencyException(ServiceCode,
            $"Service '{serviceType.FullName}' is not registered. Register it with AddBobComponents() or your own services.",
            component, null);
        e.Facts["service"] = serviceType.FullName ?? serviceType.Name;
        return e;
    }
}

/// <summary>A response with a non-success status. Carries the server's own message verbatim.</summary>
public sealed class BobHttpException : BobException
{
    public const string StatusErrorCode = "BOB-NET-001";

    public BobHttpException(string? component, string method, string url, int statusCode, string? reason, string serverMessage)
        : base(BobErrorOrigin.NetworkFailure, StatusErrorCode, serverMessage, component)
    {
        StatusCode = statusCode;
        Facts["method"] = method;
        Facts["url"] = url;
        Facts["status"] = $"{statusCode} {reason}".TrimEnd();
    }

    public int StatusCode { get; }
}

/// <summary>An app-supplied callback or render fragment threw. The original exception is the inner one.</summary>
public sealed class BobCallbackException : BobException
{
    public const string CallbackCode = "BOB-CB-001";
    public const string RenderCode = "BOB-CB-002";

    public BobCallbackException(string? component, string callbackName, Exception inner, bool isRender = false)
        : base(BobErrorOrigin.CallbackThrew, isRender ? RenderCode : CallbackCode, inner.Message, component, inner)
    {
        CallbackName = callbackName;
        Facts["callback"] = callbackName;
        var frame = BobStack.TopAppFrame(inner);
        if (frame is not null) Facts["yourFrame"] = frame;
    }

    public string CallbackName { get; }
}
