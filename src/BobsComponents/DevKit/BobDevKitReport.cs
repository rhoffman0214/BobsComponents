using System.Text;
using BobsComponents.Errors;

namespace BobsComponents.DevKit;

/// <summary>Plain-text snapshot of the dev kit panels, for pasting into a bug report or chat. Never includes headers or storage values.</summary>
public static class BobDevKitReport
{
    public static string Build(
        IReadOnlyList<BobError> errors,
        IReadOnlyList<BobNetworkEntry> network,
        IReadOnlyList<BobInteropEntry> interop,
        IReadOnlyList<BobDiRequirement> di)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Bob dev kit report");

        sb.AppendLine().AppendLine($"Errors ({errors.Count})");
        foreach (var e in errors) sb.AppendLine($"- {e.Code}: {e.Message}");

        sb.AppendLine().AppendLine($"Network ({network.Count})");
        foreach (var n in network)
            sb.AppendLine($"- {n.Method} {n.Status?.ToString() ?? "no response"} {n.Url} {(int)n.Duration.TotalMilliseconds} ms{(n.TransportError is null ? "" : " | " + n.TransportError)}");

        sb.AppendLine().AppendLine($"Interop ({interop.Count})");
        foreach (var i in interop)
            sb.AppendLine($"- {i.Identifier}({i.Arguments}) {(int)i.Duration.TotalMilliseconds} ms{(i.Failure is null ? "" : " | " + i.Failure)}");

        sb.AppendLine().AppendLine($"DI ({di.Count})");
        foreach (var d in di)
            sb.AppendLine($"- {d.Component} needs {d.ServiceType.FullName}: {(d.Registered ? "registered" : (d.Required ? "MISSING" : "missing (optional)"))}");

        return sb.ToString();
    }
}
