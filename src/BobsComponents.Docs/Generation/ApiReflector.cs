using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace BobsComponents.Docs.Generation;

public enum ApiKind
{
    /// <summary>A value parameter.</summary>
    Parameter,
    /// <summary>An <see cref="EventCallback"/>: something the component tells you about.</summary>
    Event,
    /// <summary>A <see cref="RenderFragment"/>: content you provide.</summary>
    Slot,
    /// <summary>A <see cref="CascadingParameterAttribute"/> value.</summary>
    Cascading,
    /// <summary>The catch-all for unmatched attributes (splatted onto the root).</summary>
    Attributes,
}

public sealed record ParameterDoc(string Name, string Type, string Default, string Summary, bool Required, ApiKind Kind, string DeclaredIn, bool Inherited);

public sealed record MemberDoc(string Name, string Signature, string Summary);

public sealed record ComponentApi(
    string Name,
    string FullName,
    string Summary,
    IReadOnlyList<ParameterDoc> Parameters)
{
    public IEnumerable<ParameterDoc> Of(ApiKind kind) => Parameters.Where(p => p.Kind == kind);
}

/// <summary>
/// Describes a component's public surface by reflecting over <c>[Parameter]</c> properties and joining the XML
/// documentation. The component's own summary comes from <see cref="DescriptionAttribute"/>
/// (<c>@attribute [Description("...")]</c> in the .razor file).
/// </summary>
public static class ApiReflector
{
    private const DynamicallyAccessedMemberTypes Needed =
        DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicMethods |
        DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicEvents;

    public static ComponentApi Describe([DynamicallyAccessedMembers(Needed)] Type component, XmlDocs? docs = null)
    {
        docs ??= XmlDocs.Library;
        object? instance = TryCreate(component);
        var nullability = new NullabilityInfoContext();
        var list = new List<ParameterDoc>();

        foreach (var p in component.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var param = p.GetCustomAttribute<ParameterAttribute>();
            var cascading = p.GetCustomAttribute<CascadingParameterAttribute>();
            if (param is null && cascading is null) continue;

            var kind = Classify(p.PropertyType, param, cascading);
            var required = p.GetCustomAttribute<EditorRequiredAttribute>() is not null;
            var inherited = p.DeclaringType != component;
            list.Add(new ParameterDoc(
                p.Name,
                FriendlyType(p.PropertyType, nullability.Create(p)),
                FormatDefault(p, instance, kind),
                docs.Summary(p) ?? "",
                required,
                kind,
                p.DeclaringType!.Name,
                inherited));
        }

        var ordered = list
            .OrderBy(p => p.Inherited)
            .ThenBy(p => p.Kind)
            .ThenBy(p => p.Name, StringComparer.Ordinal)
            .ToList();
        return new ComponentApi(component.Name, component.FullName ?? component.Name, Summary(component), ordered);
    }

    /// <summary>The component's one-line description from <c>[Description]</c>; empty when missing (a test fails on that).</summary>
    public static string Summary(Type component) =>
        component.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "";

    /// <summary>Public methods, properties and events declared on a service type, with XML summaries.</summary>
    public static IReadOnlyList<MemberDoc> Members([DynamicallyAccessedMembers(Needed)] Type type, XmlDocs? docs = null)
    {
        docs ??= XmlDocs.Library;
        var members = new List<MemberDoc>();
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        var nullability = new NullabilityInfoContext();

        foreach (var evt in type.GetEvents(flags))
            members.Add(new MemberDoc(evt.Name, "event " + FriendlyType(evt.EventHandlerType!, null) + " " + evt.Name, docs.Summary(evt) ?? ""));
        foreach (var prop in type.GetProperties(flags))
            members.Add(new MemberDoc(prop.Name, FriendlyType(prop.PropertyType, nullability.Create(prop)) + " " + prop.Name, docs.Summary(prop) ?? ""));
        foreach (var method in type.GetMethods(flags).Where(m => !m.IsSpecialName))
        {
            var args = string.Join(", ", method.GetParameters().Select(a =>
                FriendlyType(a.ParameterType, nullability.Create(a)) + " " + a.Name + (a.HasDefaultValue ? " = " + FormatConstant(a.DefaultValue) : "")));
            members.Add(new MemberDoc(method.Name, $"{FriendlyType(method.ReturnType, null)} {method.Name}({args})", docs.Summary(method) ?? ""));
        }
        return members;
    }

    private static object? TryCreate(Type type)
    {
        try { return Activator.CreateInstance(type); }
        catch { return null; }
    }

    private static ApiKind Classify(Type t, ParameterAttribute? param, CascadingParameterAttribute? cascading)
    {
        if (cascading is not null) return ApiKind.Cascading;
        if (param is { CaptureUnmatchedValues: true }) return ApiKind.Attributes;
        if (t == typeof(EventCallback) || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(EventCallback<>))) return ApiKind.Event;
        if (t == typeof(RenderFragment) || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(RenderFragment<>))) return ApiKind.Slot;
        return ApiKind.Parameter;
    }

    private static string FormatDefault(PropertyInfo p, object? instance, ApiKind kind)
    {
        if (kind is ApiKind.Event or ApiKind.Slot or ApiKind.Attributes or ApiKind.Cascading) return "none";
        if (instance is null) return "";
        try { return FormatConstant(p.GetValue(instance)); }
        catch { return ""; }
    }

    private static string FormatConstant(object? value) => value switch
    {
        null => "null",
        bool b => b ? "true" : "false",
        string s => "\"" + s + "\"",
        Enum e => e.GetType().Name + "." + e,
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? "",
    };

    public static string FriendlyType(Type t, NullabilityInfo? nullability)
    {
        var name = t switch
        {
            _ when t == typeof(bool) => "bool",
            _ when t == typeof(string) => "string",
            _ when t == typeof(int) => "int",
            _ when t == typeof(double) => "double",
            _ when t == typeof(void) => "void",
            _ when t == typeof(object) => "object",
            _ when Nullable.GetUnderlyingType(t) is { } inner => FriendlyType(inner, null) + "?",
            _ when t.IsGenericType => t.Name[..t.Name.IndexOf('`')] + "<" + string.Join(", ", t.GetGenericArguments().Select(a => FriendlyType(a, null))) + ">",
            _ => t.Name,
        };
        if (nullability?.ReadState == NullabilityState.Nullable && !t.IsValueType && !name.EndsWith('?')) name += "?";
        return name;
    }
}
