using System.Runtime.CompilerServices;

namespace BobsComponents.Errors;

/// <summary>Hooks for demos and tests of the error system.</summary>
public static class BobDiagnostics
{
    /// <summary>
    /// Throws from inside BobsComponents, so the classifier reports a "Component bug". Used by the error
    /// demo page and the origin tests; do not call it in app code.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowSimulatedComponentBug(string message = "Simulated bug inside a BobsComponents component.") =>
        throw new InvalidOperationException(message);
}
