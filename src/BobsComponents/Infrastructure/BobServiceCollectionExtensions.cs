using BobsComponents.DevKit;
using BobsComponents.Errors;
using BobsComponents.Options;
using BobsComponents.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BobsComponents;

public static class BobServiceCollectionExtensions
{
    /// <summary>
    /// Registers everything BobsComponents needs: options, error classifier/catalog/hub, toasts and the dev
    /// kit logs. Safe to call once; every default can be changed in <paramref name="configure"/>.
    /// </summary>
    public static IServiceCollection AddBobComponents(this IServiceCollection services, Action<BobOptions>? configure = null)
    {
        var options = new BobOptions();
        options.EnvironmentName =
            Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        configure?.Invoke(options);

        services.TryAddSingleton(options);
        services.TryAddSingleton<IBobErrorClassifier, BobErrorClassifier>();
        services.TryAddSingleton(_ => BobErrorCatalog.CreateDefault());
        services.TryAddSingleton<IBobErrorHub, BobErrorHub>();
        services.TryAddScoped<IBobToastService, BobToastService>();
        services.TryAddSingleton<IBobDraftStore, BobMemoryDraftStore>();
        services.TryAddSingleton<BobNetworkLog>();
        services.TryAddSingleton<BobInteropLog>();
        services.TryAddSingleton<BobDiRegistry>();
        services.TryAddTransient<BobNetworkHandler>();
        return services;
    }

    /// <summary>
    /// Adds the dev kit's request logging to an HttpClient: <c>services.AddHttpClient("api").AddBobNetworkLogging()</c>.
    /// For a bare <c>new HttpClient()</c> use <see cref="BobNetworkHandler"/> directly.
    /// </summary>
    public static IHttpClientBuilder AddBobNetworkLogging(this IHttpClientBuilder builder) =>
        builder.AddHttpMessageHandler<BobNetworkHandler>();

    /// <summary>
    /// Wraps the registered <see cref="Microsoft.JSInterop.IJSRuntime"/> so the dev kit Interop panel sees every
    /// call and failed import. Call after the host has registered its JS runtime (after builder creation in WASM).
    /// </summary>
    public static IServiceCollection AddBobInteropLogging(this IServiceCollection services)
    {
        var original = services.LastOrDefault(d => d.ServiceType == typeof(Microsoft.JSInterop.IJSRuntime))
            ?? throw new InvalidOperationException("No IJSRuntime is registered yet; call AddBobInteropLogging after the host registers it.");
        services.Remove(original);

        Microsoft.JSInterop.IJSRuntime Inner(IServiceProvider sp) => original switch
        {
            { ImplementationInstance: Microsoft.JSInterop.IJSRuntime i } => i,
            { ImplementationFactory: { } f } => (Microsoft.JSInterop.IJSRuntime)f(sp),
            { ImplementationType: { } t } => (Microsoft.JSInterop.IJSRuntime)ActivatorUtilities.CreateInstance(sp, t),
            _ => throw new InvalidOperationException("Unsupported IJSRuntime registration."),
        };

        services.Add(ServiceDescriptor.Describe(typeof(Microsoft.JSInterop.IJSRuntime),
            sp => new BobLoggingJSRuntime(Inner(sp), sp.GetRequiredService<BobInteropLog>(), sp.GetService<IBobErrorHub>()),
            original.Lifetime));
        return services;
    }
}
