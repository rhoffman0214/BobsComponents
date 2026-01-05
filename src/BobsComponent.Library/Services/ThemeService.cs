using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BobsComponent.Library.Services;

public class ThemeService
{
    private readonly IJSRuntime _jsRuntime;
    private string _currentTheme = "light";

    public event EventHandler<string>? ThemeChanged;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public string CurrentTheme => _currentTheme;

    public async Task SetThemeAsync(string theme)
    {
        _currentTheme = theme;
        await _jsRuntime.InvokeVoidAsync("BobsComponents.setTheme", theme);
        ThemeChanged?.Invoke(this, theme);
    }

    public async Task InitializeThemeAsync()
    {
        // Try to load saved theme from localStorage
        try
        {
            var savedTheme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "bobscomponents-theme");
            if (!string.IsNullOrEmpty(savedTheme))
            {
                _currentTheme = savedTheme;
                await _jsRuntime.InvokeVoidAsync("BobsComponents.setTheme", savedTheme);
            }
            else
            {
                await SetThemeAsync("light");
            }
        }
        catch
        {
            // If localStorage fails, just use default theme
            await SetThemeAsync("light");
        }
    }

    public async Task SaveThemeAsync(string theme)
    {
        await SetThemeAsync(theme);
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "bobscomponents-theme", theme);
        }
        catch
        {
            // Silently fail if localStorage is not available
        }
    }

    public List<ThemeInfo> GetAvailableThemes()
    {
        return new List<ThemeInfo>
        {
            new() { Id = "light", Name = "Light Mode", Description = "Clean light theme" },
            new() { Id = "dark", Name = "Dark Mode", Description = "Easy on the eyes" },
            new() { Id = "fire-nation", Name = "Fire Nation", Description = "Passion and power" },
            new() { Id = "water-tribe", Name = "Water Tribe", Description = "Calm and adaptive" },
            new() { Id = "earth-kingdom", Name = "Earth Kingdom", Description = "Strong and enduring" },
            new() { Id = "air-nomads", Name = "Air Nomads", Description = "Freedom and peace" },
            new() { Id = "accessible", Name = "Accessible", Description = "High contrast colors" }
        };
    }
}

public class ThemeInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
