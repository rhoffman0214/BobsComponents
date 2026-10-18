namespace BobsComponents.Options;

/// <summary>How much of the technical detail (stack trace, inner exceptions) an error card shows.</summary>
public enum BobErrorDetailsMode
{
    /// <summary>Details sit behind a "Details" toggle, collapsed. The default.</summary>
    Toggle,
    /// <summary>Details are always visible, no toggle.</summary>
    AlwaysOpen,
    /// <summary>No details section at all. The exact message is still shown.</summary>
    Hidden,
}

/// <summary>Where the floating dev kit button sits.</summary>
public enum BobDevKitPosition { BottomRight, BottomLeft, TopRight, TopLeft }

/// <summary>Every default in the library lives here and can be changed in <c>AddBobComponents</c>.</summary>
public sealed class BobOptions
{
    /// <summary>
    /// Host environment name, e.g. <c>builder.HostEnvironment.Environment</c>. When null the library reads
    /// DOTNET_ENVIRONMENT / ASPNETCORE_ENVIRONMENT, and treats "unknown" as not Development.
    /// </summary>
    public string? EnvironmentName { get; set; }

    public BobErrorOptions Errors { get; } = new();
    public BobDevKitOptions DevKit { get; } = new();
    public BobToastOptions Toasts { get; } = new();

    internal bool IsDevelopment =>
        string.Equals(EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase);
}

public sealed class BobErrorOptions
{
    /// <summary>Default: <see cref="BobErrorDetailsMode.Toggle"/>. The exact message is shown in every mode.</summary>
    public BobErrorDetailsMode Details { get; set; } = BobErrorDetailsMode.Toggle;
    /// <summary>Include the stack trace in the details. Default true.</summary>
    public bool ShowStackTrace { get; set; } = true;
    /// <summary>Include the inner exception chain in the details. Default true.</summary>
    public bool ShowInnerExceptions { get; set; } = true;
    /// <summary>Show the short error id to quote in a bug report. Default true.</summary>
    public bool ShowErrorId { get; set; } = true;
    /// <summary>Show a "Try again" button on boundary cards. Default true.</summary>
    public bool ShowRecoverButton { get; set; } = true;
    /// <summary>
    /// Base URL of the error catalog pages; the code is appended as a fragment, e.g.
    /// <c>/docs/errors#BOB-PARAM-001</c>. Null (default) shows the code without a link.
    /// </summary>
    public string? CatalogBaseUrl { get; set; }
}

public sealed class BobDevKitOptions
{
    /// <summary>True/false forces the dev kit on/off. Null (default) means "on only in Development".</summary>
    public bool? Enabled { get; set; }
    public bool ShowErrorsPanel { get; set; } = true;
    public bool ShowNetworkPanel { get; set; } = true;
    public bool ShowInteropPanel { get; set; } = true;
    public bool ShowDiPanel { get; set; } = true;
    /// <summary>Entries kept per panel before the oldest is dropped. Default 200.</summary>
    public int MaxEntries { get; set; } = 200;
    /// <summary>Characters of a request/response body kept. Default 4096.</summary>
    public int MaxBodyChars { get; set; } = 4096;
    public bool CaptureRequestBodies { get; set; } = true;
    public bool CaptureResponseBodies { get; set; } = true;
    /// <summary>Report exceptions from fire-and-forget tasks that no boundary saw. Default true.</summary>
    public bool CaptureUnobservedTaskExceptions { get; set; } = true;
    public BobDevKitPosition Position { get; set; } = BobDevKitPosition.BottomRight;
    public bool StartOpen { get; set; }
}

public sealed class BobToastOptions
{
    /// <summary>How long a toast stays when the caller gives no duration. Default 3 seconds.</summary>
    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromSeconds(3);
    /// <summary>Most toasts visible at once; the oldest is dropped. Default 5.</summary>
    public int MaxVisible { get; set; } = 5;
}
