namespace BobsComponents.Docs.Generation;

/// <summary>Anchor ids used on component pages, shared with the search index so result links land on the right row.</summary>
public static class DocIds
{
    public static string Var(string name) => "var-" + name.TrimStart('-');
    public static string Part(string name) => "part-" + name;
    public static string Class(string name) => "class-" + name;
    public static string Param(string name) => "param-" + name;
}
