using System.Text;

namespace BobsComponents.Docs.Generation;

/// <summary>Ready-to-paste CSS generated from a component's reference: variables with their defaults and part selectors.</summary>
public static class CssSnippets
{
    public static string VariableLine(CssVariableDoc v) => $"{v.Name}: {v.FirstFallback ?? "initial"};";

    /// <summary>A <c>:root</c> block with every component variable set to its default, ready to edit.</summary>
    public static string VariableBlock(string component, CssReference css)
    {
        var sb = new StringBuilder();
        sb.Append("/* ").Append(component).Append(": set any of these on :root or an ancestor. Shown with their defaults. */\n");
        sb.Append(":root {\n");
        foreach (var v in css.Variables) sb.Append("  ").Append(VariableLine(v)).Append('\n');
        sb.Append('}');
        return sb.ToString();
    }

    /// <summary>The selector for a part: its class plus the part attribute, so it wins inside the library's layer without !important.</summary>
    public static string PartSelector(PartDoc part) =>
        part.Classes.Count > 0 ? $".{part.Classes[0]}[data-bob-part=\"{part.Name}\"]" : $"[data-bob-part=\"{part.Name}\"]";

    public static string PartRule(PartDoc part) => PartSelector(part) + " {\n  \n}";

    /// <summary>One empty rule per part, in DOM-ish order.</summary>
    public static string PartBlock(string component, CssReference css)
    {
        var sb = new StringBuilder();
        sb.Append("/* ").Append(component).Append(": one rule per part. Unlayered CSS beats the library's @layer bob. */\n");
        foreach (var part in css.Parts) sb.Append(PartRule(part)).Append("\n\n");
        return sb.ToString().TrimEnd();
    }
}
