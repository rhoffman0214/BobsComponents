using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System;
using BobsComponent.Client;
using BobsComponent.Library.Services;
using BobsComponents;
using BobsComponents.Docs;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure logging for browser console
builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Logging.AddFilter("BobsComponent", LogLevel.Debug);
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register MicroActions services
builder.Services.AddScoped<ActionQueueService>();
builder.Services.AddScoped<MockApiService>(sp =>
{
    var httpClient = new HttpClient();
    return new MockApiService(httpClient);
});

// Register Theme service
builder.Services.AddScoped<ThemeService>();

// Register Clipboard service (for CodeSnippet component)
builder.Services.AddScoped<ClipboardService>();

// BobsComponents: error system, toasts and dev kit logs. The dev kit is on here on purpose (it is a docs site);
// by default it is off outside Development.
builder.Services.AddBobComponents(o =>
{
    o.EnvironmentName = builder.HostEnvironment.Environment;
    o.DevKit.Enabled = true;
    o.Errors.CatalogBaseUrl = "bob/error-catalog";
});
builder.Services.AddHttpClient("bob-demo", c => c.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)).AddBobNetworkLogging();
builder.Services.AddBobInteropLogging();
builder.Services.AddBobDocs();

var host = builder.Build();

// Initialize theme on startup
var themeService = host.Services.GetRequiredService<ThemeService>();
await themeService.InitializeThemeAsync();

await host.RunAsync();
