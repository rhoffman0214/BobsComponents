using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BobsComponent.Library.Components;
using BobsComponent.Library.Enums;
using BobsComponent.Library.Services;
using Microsoft.AspNetCore.Components;

namespace BobsComponent.Client.Pages;

public partial class MicroActions : IDisposable
{
    private Random _random = new();
    private ErrorDisplay? errorDisplayRef;
    private bool showOverlay = false;
    private bool isLoadingData = false;

    // Progress tracking items
    private List<ProgressItem> ProgressItems { get; set; } = new();

    // Sample data for skeleton loading demo
    private List<SampleUser> SampleUsers { get; set; } = new();

    // Pre-assigned button styles (assigned once, never change)
    private ButtonStyle[] SuccessButtonStyles = new ButtonStyle[3];
    private ButtonStyle[] ErrorButtonStyles = new ButtonStyle[3];
    private ButtonStyle[] ConcurrentButtonStyles = new ButtonStyle[50];

    // References to the 50 concurrent action buttons
    private AsyncButton?[] ConcurrentButtons = new AsyncButton?[50];

    // Computed property for completed actions count
    private int CompletedCount => QueueService.Actions.Count(a =>
        a.State == LoadingState.Success || a.State == LoadingState.Error);

    protected override void OnInitialized()
    {
        // Subscribe to queue changes
        QueueService.QueueChanged += OnQueueChanged;

        // Initialize progress items
        ProgressItems = new List<ProgressItem>
        {
            new() { Name = "Upload File", Style = ButtonStyle.Primary },
            new() { Name = "Process Data", Style = ButtonStyle.Tertiary },
            new() { Name = "Generate Report", Style = ButtonStyle.Success }
        };

        // Initialize sample users
        SampleUsers = new List<SampleUser>
        {
            new() { Id = 1, Name = "John Doe", Email = "john.doe@example.com" },
            new() { Id = 2, Name = "Jane Smith", Email = "jane.smith@example.com" },
            new() { Id = 3, Name = "Bob Johnson", Email = "bob.johnson@example.com" },
            new() { Id = 4, Name = "Alice Williams", Email = "alice.williams@example.com" },
            new() { Id = 5, Name = "Charlie Brown", Email = "charlie.brown@example.com" }
        };

        // Pre-assign random button styles (so they don't change on every render)
        for (int i = 0; i < 3; i++)
        {
            SuccessButtonStyles[i] = GetRandomButtonStyle();
            ErrorButtonStyles[i] = GetRandomButtonStyle();
        }

        for (int i = 0; i < 50; i++)
        {
            ConcurrentButtonStyles[i] = GetRandomButtonStyle();
        }
    }

    private void OnQueueChanged(object? sender, EventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private ButtonStyle GetRandomButtonStyle()
    {
        var styles = Enum.GetValues<ButtonStyle>();
        return styles[_random.Next(styles.Length)];
    }

    // Progress tracking handler
    private async Task HandleProgressAction(ProgressItem item)
    {
        var progress = new Progress<int>(value =>
        {
            item.Progress = value;
            InvokeAsync(async () =>
            {
                if (item.ButtonRef != null)
                {
                    await item.ButtonRef.UpdateProgressAsync(value);
                }
                StateHasChanged();
            });
        });

        await ApiService.SlowOperationAsync(progress);
        item.Progress = 0; // Reset after completion
    }

    // Queue management handlers
    private async Task HandleQueuedAction(string actionName)
    {
        var metadata = QueueService.RegisterAction(actionName);
        if (metadata == null)
        {
            // Action limit reached
            return;
        }

        try
        {
            // Add random delay between 500ms and 3000ms for variety
            var randomDelay = _random.Next(500, 3001);
            await Task.Delay(randomDelay);

            QueueService.UpdateActionState(metadata.Id, LoadingState.Success);
        }
        catch
        {
            QueueService.UpdateActionState(metadata.Id, LoadingState.Error, "Action failed");
        }

        // Auto cleanup after 3 seconds
        await Task.Delay(3000);
        QueueService.CleanupCompletedActions();
    }

    private async Task ClearAllActions(CancellationToken ct = default)
    {
        QueueService.ClearAll();

        // Reset all concurrent buttons to idle state
        for (int i = 0; i < 50; i++)
        {
            if (ConcurrentButtons[i] != null)
            {
                await ConcurrentButtons[i]!.ResetAsync();
            }
        }
    }

    private async Task StartAllActions(CancellationToken ct = default)
    {
        // This will attempt to start all 50 actions simultaneously
        // Demonstrating the queue limit
        for (int i = 0; i < 50; i++)
        {
            if (ConcurrentButtons[i] != null)
            {
                // Fire and forget - don't await
                _ = ConcurrentButtons[i]!.TriggerClickAsync();
                // Small delay to allow UI updates
                await Task.Delay(10, ct);
            }
        }
    }

    // Skeleton loading demo
    private void ToggleDataLoading()
    {
        isLoadingData = !isLoadingData;
    }

    // Loading overlay demo
    private async Task HandleOverlayAction(CancellationToken ct = default)
    {
        showOverlay = true;
        StateHasChanged();

        await Task.Delay(3000, ct);

        showOverlay = false;
        StateHasChanged();
    }

    public void Dispose()
    {
        QueueService.QueueChanged -= OnQueueChanged;
    }

    // Helper classes
    public class ProgressItem
    {
        public string Name { get; set; } = string.Empty;
        public ButtonStyle Style { get; set; }
        public int Progress { get; set; } = 0;
        public AsyncButton? ButtonRef { get; set; }
    }

    public class SampleUser
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
