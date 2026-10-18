using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BobsComponents.Docs.Generation;

/// <summary>
/// Reads a compiler-generated XML documentation file and answers "what is the summary of this member?".
/// Inline markup becomes plain text with backticks around code (<c>&lt;c&gt;</c>, <c>&lt;see cref&gt;</c>,
/// <c>&lt;paramref&gt;</c>); the page turns backticks into code elements.
/// </summary>
public sealed partial class XmlDocs
{
    private readonly Dictionary<string, string> _summaries = new(StringComparer.Ordinal);

    public static XmlDocs Load(string xml)
    {
        var docs = new XmlDocs();
        var doc = XDocument.Parse(xml);
        foreach (var member in doc.Descendants("member"))
        {
            var name = member.Attribute("name")?.Value;
            var summary = member.Element("summary");
            if (name is null || summary is null) continue;
            var text = Render(summary);
            if (text.Length > 0) docs._summaries[name] = text;
        }
        return docs;
    }

    private static readonly Lazy<XmlDocs> LibraryDocs = new(() => Load(DocSources.XmlDocs()));

    /// <summary>The documentation of the BobsComponents library, loaded once from the embedded XML file.</summary>
    public static XmlDocs Library => LibraryDocs.Value;

    public int Count => _summaries.Count;

    public string? Summary(Type type) => _summaries.GetValueOrDefault("T:" + TypeKey(type));

    public string? Summary(PropertyInfo property) =>
        _summaries.GetValueOrDefault($"P:{TypeKey(property.DeclaringType!)}.{property.Name}");

    public string? Summary(MethodInfo method)
    {
        var prefix = $"M:{TypeKey(method.DeclaringType!)}.{method.Name}";
        var exact = prefix + "(" + string.Join(",", method.GetParameters().Select(p => ParamKey(p.ParameterType))) + ")";
        if (_summaries.TryGetValue(exact, out var s)) return s;
        if (method.GetParameters().Length == 0 && _summaries.TryGetValue(prefix, out s)) return s;
        // Fall back to the first overload with this name; good enough for the small service surfaces documented here.
        return _summaries.FirstOrDefault(kv => kv.Key == prefix || kv.Key.StartsWith(prefix + "(", StringComparison.Ordinal)).Value;
    }

    public string? Summary(EventInfo evt) =>
        _summaries.GetValueOrDefault($"E:{TypeKey(evt.DeclaringType!)}.{evt.Name}");

    private static string TypeKey(Type t) => (t.FullName ?? t.Name).Replace('+', '.');

    private static string ParamKey(Type t)
    {
        if (t.IsByRef) return ParamKey(t.GetElementType()!) + "@";
        if (t.IsArray) return ParamKey(t.GetElementType()!) + "[]";
        if (t.IsGenericType)
        {
            var def = t.GetGenericTypeDefinition();
            var name = TypeKey(def);
            name = name[..name.IndexOf('`')];
            return name + "{" + string.Join(",", t.GetGenericArguments().Select(ParamKey)) + "}";
        }
        return t.IsGenericParameter ? "`" + t.GenericParameterPosition : TypeKey(t);
    }

    private static string Render(XElement element)
    {
        var sb = new StringBuilder();
        foreach (var node in element.Nodes()) RenderNode(node, sb);
        return WhitespaceRegex().Replace(sb.ToString(), " ").Trim();
    }

    private static void RenderNode(XNode node, StringBuilder sb)
    {
        switch (node)
        {
            case XText text:
                sb.Append(text.Value);
                break;
            case XElement el:
                switch (el.Name.LocalName)
                {
                    case "see" or "seealso":
                        var langword = el.Attribute("langword")?.Value;
                        var cref = el.Attribute("cref")?.Value;
                        sb.Append('`').Append(langword ?? CrefName(cref) ?? el.Value).Append('`');
                        break;
                    case "paramref" or "typeparamref":
                        sb.Append('`').Append(el.Attribute("name")?.Value).Append('`');
                        break;
                    case "c" or "code":
                        sb.Append('`').Append(el.Value).Append('`');
                        break;
                    default:
                        foreach (var child in el.Nodes()) RenderNode(child, sb);
                        break;
                }
                break;
        }
    }

    private static string? CrefName(string? cref)
    {
        if (cref is null) return null;
        var name = cref.Length > 2 && cref[1] == ':' ? cref[2..] : cref;
        var paren = name.IndexOf('(');
        if (paren >= 0) name = name[..paren];
        var dot = name.LastIndexOf('.');
        return dot >= 0 ? name[(dot + 1)..] : name;
    }

    [GeneratedRegex(@"\s+")] private static partial Regex WhitespaceRegex();
}
