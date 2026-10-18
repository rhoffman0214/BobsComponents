using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace BobsComponents.DevKit;

/// <summary>
/// Declares a service a component needs. The dev kit DI panel lists these and flags the ones that are not
/// registered. Put it on the component class; it can be repeated.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class BobRequiresServiceAttribute : Attribute
{
    public BobRequiresServiceAttribute(Type serviceType) => ServiceType = serviceType;
    public Type ServiceType { get; }
    /// <summary>False when the component works without it (it only loses a feature).</summary>
    public bool Required { get; init; } = true;
}

public sealed record BobDiRequirement(string Component, Type ServiceType, bool Required, bool Registered);

/// <summary>Finds <see cref="BobRequiresServiceAttribute"/> declarations and checks them against the container.</summary>
public sealed class BobDiRegistry
{
    private readonly IServiceProvider _services;
    private readonly List<Assembly> _extraAssemblies = [];

    public BobDiRegistry(IServiceProvider services) => _services = services;

    /// <summary>Also scan this assembly (an app's own components). BobsComponents is always scanned.</summary>
    public void AddAssembly(Assembly assembly)
    {
        if (!_extraAssemblies.Contains(assembly)) _extraAssemblies.Add(assembly);
    }

    public IReadOnlyList<BobDiRequirement> Check()
    {
        var isService = _services.GetService<IServiceProviderIsService>();
        var results = new List<BobDiRequirement>();
        foreach (var assembly in new[] { typeof(BobDiRegistry).Assembly }.Concat(_extraAssemblies))
        {
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t is not null).ToArray()!; }

            foreach (var type in types)
            {
                foreach (var attr in type.GetCustomAttributes<BobRequiresServiceAttribute>(inherit: true))
                {
                    var registered = isService?.IsService(attr.ServiceType)
                        ?? _services.GetService(attr.ServiceType) is not null;
                    results.Add(new BobDiRequirement(type.Name, attr.ServiceType, attr.Required, registered));
                }
            }
        }
        return results.OrderBy(r => r.Component).ThenBy(r => r.ServiceType.Name).ToList();
    }
}
