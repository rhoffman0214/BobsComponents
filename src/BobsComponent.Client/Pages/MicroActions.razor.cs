using System;
using System.Collections.Generic;
using System.Linq;
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
            new() { Name = "Process Data", Style = ButtonStyle.Info },
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
            await ApiService.MediumOperationAsync();
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

    private void ClearAllActions()
    {
        QueueService.ClearAll();
    }

    private async Task StartAllActions()
    {
        // This will attempt to start all 50 actions simultaneously
        // Demonstrating the queue limit
        var tasks = new List<Task>();
        for (int i = 0; i < 50; i++)
        {
            var actionName = $"Bulk Action {i + 1}";
            tasks.Add(HandleQueuedAction(actionName));
        }
        await Task.WhenAll(tasks);
    }

    // Skeleton loading demo
    private void ToggleDataLoading()
    {
        isLoadingData = !isLoadingData;
    }

    // Loading overlay demo
    private async Task HandleOverlayAction()
    {
        showOverlay = true;
        StateHasChanged();

        await Task.Delay(3000);

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
