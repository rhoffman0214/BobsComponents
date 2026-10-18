namespace BobsComponents.Docs.Generation;

public enum ClassKind { Block, Element, Modifier, Utility }

/// <summary>One place a custom property is read: the property it feeds, on which element and in which state.</summary>
public sealed record VariableUse(string Property, string Selector, string Target, string? Part, string State)
{
    /// <summary>Plain-language effect, e.g. "background of sheet" or "color of icon when inside .bob-toast--success".</summary>
    public string Effect => State.Length == 0 ? $"{Property} of {Target}" : $"{Property} of {Target} when {State}";
}

public sealed record CssVariableDoc(string Name, string Default, IReadOnlyList<VariableUse> Uses, IReadOnlyList<string> SetBy, string? FirstFallback = null)
{
    public string Effects => string.Join("; ", Uses.Select(u => u.Effect).Distinct());
}

public sealed record TokenUse(string Name, string Light, string? Dark);

/// <summary>One declaration in a rule and the component variables its value reads.</summary>
public sealed record RuleDeclaration(string Property, string Value, IReadOnlyList<string> Variables);

public sealed record StyleRule(
    string Selector,
    string Target,
    string? Part,
    string State,
    IReadOnlyList<string> Properties,
    IReadOnlyList<string> Variables,
    IReadOnlyList<string> SubjectClasses,
    IReadOnlyList<RuleDeclaration> Declarations);

public sealed record ClassDoc(string Name, ClassKind Kind, string DefinedIn, IReadOnlyList<string> Parts, int RuleCount);

public sealed record PartDoc(
    string Name,
    string Element,
    IReadOnlyList<string> Classes,
    IReadOnlyList<string> Variants,
    IReadOnlyList<string> Variables,
    IReadOnlyList<StyleRule> Rules);

/// <summary>The global design tokens, read from <c>css/tokens.css</c>.</summary>
public sealed class TokenSheet
{
    public IReadOnlyDictionary<string, string> Light { get; }
    public IReadOnlyDictionary<string, string> Dark { get; }

    private TokenSheet(Dictionary<string, string> light, Dictionary<string, string> dark)
    {
        Light = light;
        Dark = dark;
    }

    public bool IsToken(string name) => Light.ContainsKey(name) || Dark.ContainsKey(name);

    public static TokenSheet Parse(string css)
    {
        var sheet = CssParser.Parse("css/tokens.css", css);
        var light = new Dictionary<string, string>(StringComparer.Ordinal);
        var dark = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var rule in sheet.Rules)
        {
            var isDark = rule.Condition?.Contains("dark", StringComparison.Ordinal) == true
                         || rule.SelectorText.Contains("\"dark\"", StringComparison.Ordinal);
            foreach (var decl in rule.Declarations.Where(d => d.IsCustomPropertyDeclaration))
            {
                if (isDark) dark[decl.Property] = decl.Value;
                else light.TryAdd(decl.Property, decl.Value);
            }
        }
        return new TokenSheet(light, dark);
    }

    private static readonly Lazy<TokenSheet> Shared = new(() => Parse(DocSources.Css("css/tokens.css")));
    public static TokenSheet Library => Shared.Value;
}

/// <summary>
/// Everything the CSS reference says about one component, derived from its stylesheet, its markup and the token
/// sheet: variables with defaults and where each is used, global tokens it falls back to, classes, parts, and
/// every rule with the state that triggers it.
/// </summary>
public sealed class CssReference
{
    public required string SheetFile { get; init; }
    public required IReadOnlyList<CssVariableDoc> Variables { get; init; }
    public required IReadOnlyList<TokenUse> Tokens { get; init; }
    public required IReadOnlyList<ClassDoc> Classes { get; init; }
    public required IReadOnlyList<PartDoc> Parts { get; init; }
    public required IReadOnlyList<StyleRule> Rules { get; init; }
    public required IReadOnlyList<string> Keyframes { get; init; }

    /// <summary>Class-name prefixes the component completes at run time (for example <c>bob-button--</c> + the variant).</summary>
    public required IReadOnlyList<string> DynamicClassPatterns { get; init; }

    private static readonly Lazy<IReadOnlyList<CssSheet>> AllSheets = new(() =>
        DocSources.Names("src/wwwroot/css/")
            .Where(n => !n.EndsWith("tokens.css", StringComparison.Ordinal))
            .Select(n => CssParser.Parse(n["src/wwwroot/".Length..], DocSources.Read(n)))
            .ToList());

    /// <summary>Every library sheet except the tokens, parsed once; used to find where a class is defined.</summary>
    public static IReadOnlyList<CssSheet> LibrarySheets => AllSheets.Value;

    public static CssReference Build(string sheetFile, MarkupInfo markup, TokenSheet tokens)
    {
        var sheet = CssParser.Parse(sheetFile, DocSources.Css(sheetFile));
        return Build(sheet, markup, tokens, LibrarySheets.Where(s => s.File != sheetFile).ToList());
    }

    public static CssReference Build(CssSheet sheet, MarkupInfo markup, TokenSheet tokens, IReadOnlyList<CssSheet>? otherSheets = null)
    {
        var partByClass = MapClassesToParts(markup, sheet);
        var rules = new List<StyleRule>();
        var varInfos = new Dictionary<string, (List<(string Value, string? Condition)> Fallbacks, List<VariableUse> Uses, List<string> SetBy)>(StringComparer.Ordinal);
        var tokenNames = new List<string>();
        var order = new List<string>();

        void Touch(string name)
        {
            if (varInfos.ContainsKey(name)) return;
            varInfos[name] = ([], [], []);
            order.Add(name);
        }

        foreach (var rule in sheet.Rules)
        {
            foreach (var selector in rule.Selectors)
            {
                var (target, part) = TargetOf(selector, partByClass);
                var state = StateOf(selector, rule.Condition);

                foreach (var decl in rule.Declarations)
                {
                    if (decl.IsCustomPropertyDeclaration && !tokens.IsToken(decl.Property))
                    {
                        Touch(decl.Property);
                        varInfos[decl.Property].SetBy.Add(selector.Text + " → " + decl.Value);
                    }

                    foreach (var read in decl.VarRefs)
                    {
                        if (tokens.IsToken(read.Name))
                        {
                            if (!tokenNames.Contains(read.Name)) tokenNames.Add(read.Name);
                            continue;
                        }
                        if (decl.IsCustomPropertyDeclaration) continue;
                        Touch(read.Name);
                        var info = varInfos[read.Name];
                        if (read.Fallback is not null)
                        {
                            var entry = (read.Fallback, rule.Condition is null ? null : Condition(rule.Condition));
                            if (!info.Fallbacks.Contains(entry)) info.Fallbacks.Add(entry);
                        }
                        info.Uses.Add(new VariableUse(decl.Property, selector.Text, target, part, state));
                    }
                }

                var vars = rule.Declarations
                    .SelectMany(d => d.VarRefs.Select(v => v.Name).Concat(d.IsCustomPropertyDeclaration ? [d.Property] : []))
                    .Where(n => !tokens.IsToken(n))
                    .Distinct()
                    .ToList();
                var declarations = rule.Declarations
                    .Select(d => new RuleDeclaration(d.Property, d.Value,
                        d.VarRefs.Select(v => v.Name).Where(n => !tokens.IsToken(n)).Distinct().ToList()))
                    .ToList();
                rules.Add(new StyleRule(selector.Text, target, part, state,
                    rule.Declarations.Where(d => !d.IsCustomPropertyDeclaration).Select(d => d.Property).Distinct().ToList(),
                    vars, selector.Subject.Classes, declarations));
            }
        }

        var variables = order
            .Select(n => varInfos[n])
            .Zip(order, (info, name) => new CssVariableDoc(
                name,
                info.Fallbacks.Count == 0 ? "(unset)" : string.Join("; ", info.Fallbacks.Select(f => f.Condition is null ? f.Value : $"{f.Value} at {f.Condition}")),
                info.Uses,
                info.SetBy,
                info.Fallbacks.Count == 0 ? null : info.Fallbacks[0].Value))
            .ToList();

        var tokenUses = tokenNames
            .Select(n => new TokenUse(n, tokens.Light.GetValueOrDefault(n, ""), tokens.Dark.GetValueOrDefault(n)))
            .ToList();

        var classes = BuildClasses(sheet, markup, otherSheets ?? [], partByClass);
        var parts = BuildParts(markup, sheet, rules, classes);

        return new CssReference
        {
            SheetFile = sheet.File,
            Variables = variables,
            Tokens = tokenUses,
            Classes = classes,
            Parts = parts,
            Rules = rules,
            Keyframes = sheet.Keyframes,
            DynamicClassPatterns = markup.ClassPrefixes,
        };
    }

    // ---------------------------------------------------------------- helpers

    private static Dictionary<string, string> MapClassesToParts(MarkupInfo markup, CssSheet sheet)
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var part in markup.Parts)
        {
            foreach (var c in part.Classes) map.TryAdd(c, part.Name);
            foreach (var prefix in part.ClassPrefixes)
                foreach (var c in SheetClasses(sheet).Where(c => c.StartsWith(prefix, StringComparison.Ordinal)))
                    map.TryAdd(c, part.Name);
        }
        return map;
    }

    private static IEnumerable<string> SheetClasses(CssSheet sheet) =>
        sheet.Rules.SelectMany(r => r.Selectors).SelectMany(s => s.Compounds).SelectMany(c => c.Classes).Distinct();

    /// <summary>What a selector styles: the part's name when known, otherwise the class.</summary>
    private static (string Target, string? Part) TargetOf(SelectorInfo selector, Dictionary<string, string> partByClass)
    {
        var subject = selector.Subject;
        var part = subject.Part;
        if (part is null)
            foreach (var c in subject.Classes)
                if (partByClass.TryGetValue(c, out var p)) { part = p; break; }
        var cls = subject.Classes.FirstOrDefault(c => !c.Contains("--", StringComparison.Ordinal)) ?? subject.Classes.FirstOrDefault();
        var target = part ?? (cls is null ? selector.Text : "." + cls);
        return (target, part);
    }

    /// <summary>The conditions under which a rule applies, in words: modifiers, pseudo-classes, attributes, ancestors, media.</summary>
    public static string StateOf(SelectorInfo selector, string? condition)
    {
        var bits = new List<string>();
        var subject = selector.Subject;
        bits.AddRange(subject.Classes.Where(c => c.Contains("--", StringComparison.Ordinal)).Select(c => "." + c));
        bits.AddRange(subject.Pseudos);
        bits.AddRange(subject.Attributes.Where(a => !a.StartsWith("data-bob-part", StringComparison.Ordinal)).Select(a => $"[{a}]"));
        foreach (var ancestor in selector.Compounds.Take(selector.Compounds.Count - 1))
            bits.Add("inside " + ancestor.Text);
        if (condition is not null) bits.Add(Condition(condition));
        return string.Join(", ", bits);
    }

    private static string Condition(string condition) =>
        string.Join(" > ", condition.Split(" > ").Select(c => c.StartsWith("@media ", StringComparison.Ordinal) ? c["@media ".Length..] : c));

    private static List<ClassDoc> BuildClasses(CssSheet sheet, MarkupInfo markup, IReadOnlyList<CssSheet> otherSheets, Dictionary<string, string> partByClass)
    {
        var names = new List<string>();
        foreach (var c in SheetClasses(sheet).Where(c => c.StartsWith("bob-", StringComparison.Ordinal))) if (!names.Contains(c)) names.Add(c);
        foreach (var c in markup.Classes) if (!names.Contains(c)) names.Add(c);
        foreach (var prefix in markup.ClassPrefixes)
            foreach (var c in SheetClasses(sheet).Where(c => c.StartsWith(prefix, StringComparison.Ordinal))) if (!names.Contains(c)) names.Add(c);

        var sheetClasses = SheetClasses(sheet).ToHashSet();

        return names.Select(n =>
        {
            var elsewhere = otherSheets.FirstOrDefault(s => SheetClasses(s).Contains(n));
            var kind = elsewhere?.File == "css/base.css" && !sheetClasses.Contains(n) ? ClassKind.Utility
                : n.Contains("__", StringComparison.Ordinal) ? ClassKind.Element
                : n.Contains("--", StringComparison.Ordinal) ? ClassKind.Modifier
                : ClassKind.Block;
            var definedIn = sheetClasses.Contains(n) ? sheet.File : elsewhere?.File ?? HookOnly;
            var parts = markup.Parts.Where(p => p.Classes.Contains(n)).Select(p => p.Name).ToList();
            if (parts.Count == 0 && partByClass.TryGetValue(n, out var inferred)) parts.Add(inferred);
            var ruleCount = sheet.Rules.Count(r => r.Selectors.Any(s => s.Compounds.Any(c => c.Classes.Contains(n))));
            return new ClassDoc(n, kind, definedIn, parts, ruleCount);
        }).ToList();
    }

    /// <summary>Shown as "defined in" for a class the component writes but no stylesheet styles: a hook for your own CSS.</summary>
    public const string HookOnly = "(hook: no default rule)";

    private static List<PartDoc> BuildParts(MarkupInfo markup, CssSheet sheet, List<StyleRule> rules, List<ClassDoc> classes)
    {
        var result = new List<PartDoc>();
        foreach (var part in markup.Parts)
        {
            var variants = part.ClassPrefixes
                .SelectMany(prefix => classes.Where(c => c.Name.StartsWith(prefix, StringComparison.Ordinal)).Select(c => c.Name))
                .Distinct().ToList();
            var related = rules.Where(r => r.Part == part.Name).ToList();
            var variables = related.SelectMany(r => r.Variables).Distinct().ToList();
            result.Add(new PartDoc(part.Name, part.Element, part.Classes, variants, variables, related));
        }
        return result;
    }
}
