using BobsComponents.Options;

namespace BobsComponents.DevKit;

internal static class BobDevKitState
{
    /// <summary>Enabled flag if set, otherwise "environment is Development".</summary>
    public static bool IsEnabled(BobOptions options) => options.DevKit.Enabled ?? options.IsDevelopment;
}
