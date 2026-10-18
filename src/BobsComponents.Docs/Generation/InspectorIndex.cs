using System.Text.Json;

namespace BobsComponents.Docs.Generation;

/// <summary>
/// The lookup the part inspector uses in the browser: for every class, the component it belongs to, the custom
/// properties its rules read (with the property each feeds and the state) and the states that change it. Built from
/// the same <see cref="CssReference"/> as the tables, so the tooltip cannot disagree with the page.
/// </summary>
public static class InspectorIndex
{
    public static string BuildJson(IEnumerable<ComponentDocModel> models)
    {
        var index = new Dictionary<string, Entry>(StringComparer.Ordinal);
        foreach (var model in models)
        {
            foreach (var rule in model.Css.Rules)
            {
                foreach (var cls in rule.SubjectClasses)
                {
                    if (!index.TryGetValue(cls, out var entry))
                    {
                        entry = new Entry(model.Doc.Title);
                        index[cls] = entry;
                    }
                    foreach (var decl in rule.Declarations)
                        foreach (var v in decl.Variables)
                        {
                            var item = new[] { v, decl.Property.StartsWith("--", StringComparison.Ordinal) ? "(default for variants)" : decl.Property, rule.State };
                            if (!entry.Vars.Any(x => x.SequenceEqual(item))) entry.Vars.Add(item);
                        }
                    if (rule.State.Length > 0)
                    {
                        var props = string.Join(", ", rule.Properties.Concat(rule.Declarations.Where(d => d.Property.StartsWith("--", StringComparison.Ordinal)).Select(d => d.Property)).Distinct());
                        if (props.Length > 0 && !entry.States.Any(s => s[0] == rule.State)) entry.States.Add([rule.State, props]);
                    }
                }
            }
        }
        return JsonSerializer.Serialize(index, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public sealed class Entry(string component)
    {
        public string Component { get; } = component;
        public List<string[]> Vars { get; } = [];
        public List<string[]> States { get; } = [];
    }
}
