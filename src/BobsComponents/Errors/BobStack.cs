using System.Diagnostics;
using System.Reflection;

namespace BobsComponents.Errors;

/// <summary>
/// Stack inspection used to decide whether a library bug or app code threw. It looks at assemblies of
/// frames, never at message text.
/// </summary>
internal static class BobStack
{
    private static readonly Assembly LibraryAssembly = typeof(BobStack).Assembly;

    private static readonly string[] FrameworkPrefixes =
        ["System.", "Microsoft.", "mscorlib", "netstandard", "System", "xunit", "Bunit", "bunit"];

    private static bool IsFramework(Assembly? assembly)
    {
        var name = assembly?.GetName().Name;
        if (name is null) return true;
        foreach (var prefix in FrameworkPrefixes)
        {
            if (name.StartsWith(prefix, StringComparison.Ordinal)) return true;
        }
        return false;
    }

    /// <summary>First stack frame that is neither framework nor test-harness code, or null if none can be resolved.</summary>
    public static StackFrame? FirstOwnedFrame(Exception exception)
    {
        var frames = new StackTrace(exception, false).GetFrames();
        foreach (var frame in frames)
        {
            var type = frame.GetMethod()?.DeclaringType;
            if (type is null || IsFramework(type.Assembly)) continue;
            return frame;
        }
        return null;
    }

    /// <summary>True when the first owned frame is in BobsComponents, or when no frame can be resolved.</summary>
    public static bool ThrownByLibrary(Exception exception)
    {
        var frame = FirstOwnedFrame(exception);
        if (frame is null) return true;
        return frame.GetMethod()?.DeclaringType?.Assembly == LibraryAssembly;
    }

    /// <summary>"Type.Method" of the first app-owned frame, for the callback origin's "your top stack frame".</summary>
    public static string? TopAppFrame(Exception exception)
    {
        var frame = FirstOwnedFrame(exception);
        var method = frame?.GetMethod();
        if (method is null) return null;
        var file = frame!.GetFileName();
        var where = file is null ? "" : $" ({Path.GetFileName(file)}:{frame.GetFileLineNumber()})";
        return $"{method.DeclaringType?.FullName}.{method.Name}{where}";
    }
}
