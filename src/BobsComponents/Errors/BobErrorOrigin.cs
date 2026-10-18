namespace BobsComponents.Errors;

/// <summary>Whose fault an error is. Every <see cref="BobError"/> has exactly one.</summary>
public enum BobErrorOrigin
{
    /// <summary>A bug inside a BobsComponents component.</summary>
    ComponentBug,
    /// <summary>A parameter was missing, invalid or conflicted with another.</summary>
    WrongParameters,
    /// <summary>Data did not have the shape the component expected.</summary>
    DataShapeMismatch,
    /// <summary>A script, JS function or service the component needs is missing.</summary>
    MissingAssetOrService,
    /// <summary>A network call failed.</summary>
    NetworkFailure,
    /// <summary>Code supplied by the app (an EventCallback or RenderFragment) threw.</summary>
    CallbackThrew,
}

public static class BobErrorOriginExtensions
{
    /// <summary>Short badge text shown on the error card.</summary>
    public static string Badge(this BobErrorOrigin origin) => origin switch
    {
        BobErrorOrigin.ComponentBug => "Component bug",
        BobErrorOrigin.WrongParameters => "Wrong parameters",
        BobErrorOrigin.DataShapeMismatch => "Data shape mismatch",
        BobErrorOrigin.MissingAssetOrService => "Missing asset or service",
        BobErrorOrigin.NetworkFailure => "Network failure",
        BobErrorOrigin.CallbackThrew => "Your callback threw",
        _ => origin.ToString(),
    };

    /// <summary>One plain sentence saying whose fault it is.</summary>
    public static string Blame(this BobErrorOrigin origin) => origin switch
    {
        BobErrorOrigin.ComponentBug => "This is a bug in BobsComponents, not in your code.",
        BobErrorOrigin.WrongParameters => "A component was given parameters it cannot use.",
        BobErrorOrigin.DataShapeMismatch => "The data does not have the shape the component expected.",
        BobErrorOrigin.MissingAssetOrService => "Something the component depends on is not available.",
        BobErrorOrigin.NetworkFailure => "A network call failed.",
        BobErrorOrigin.CallbackThrew => "Code you passed to the component threw an exception.",
        _ => "",
    };

    /// <summary>CSS-friendly name used in <c>bob-error-card--origin-*</c>.</summary>
    public static string CssName(this BobErrorOrigin origin) => origin switch
    {
        BobErrorOrigin.ComponentBug => "component-bug",
        BobErrorOrigin.WrongParameters => "wrong-parameters",
        BobErrorOrigin.DataShapeMismatch => "data-shape",
        BobErrorOrigin.MissingAssetOrService => "missing",
        BobErrorOrigin.NetworkFailure => "network",
        BobErrorOrigin.CallbackThrew => "callback",
        _ => "unknown",
    };
}
