using System;
using System.Collections.Generic;
using System.Linq;
using BobsComponent.Library.Enums;
using BobsComponent.Library.Models;

namespace BobsComponent.Library.Services;

/// <summary>
/// Manages a queue of asynchronous actions with a concurrent action limit
/// </summary>
public class ActionQueueService
{
    private const int MaxConcurrentActions = 50;
    private readonly List<ActionMetadata> _actions = new();
    private readonly object _lock = new();

    /// <summary>
    /// Event triggered when the action queue changes
    /// </summary>
    public event EventHandler? QueueChanged;

    /// <summary>
    /// Gets all current actions
    /// </summary>
    public IReadOnlyList<ActionMetadata> Actions
    {
        get
        {
            lock (_lock)
            {
                return _actions.ToList();
            }
        }
    }

    /// <summary>
    /// Gets the count of currently running actions
    /// </summary>
    public int RunningCount
    {
        get
        {
            lock (_lock)
            {
                return _actions.Count(a => a.State == LoadingState.Loading);
            }
        }
    }

    /// <summary>
    /// Gets whether the concurrent action limit has been reached
    /// </summary>
    public bool IsAtLimit => RunningCount >= MaxConcurrentActions;

    /// <summary>
    /// Registers a new action in the queue
    /// </summary>
    /// <param name="actionName">Name of the action</param>
    /// <returns>Action metadata if registered successfully, null if limit reached</returns>
    public ActionMetadata? RegisterAction(string actionName)
    {
        lock (_lock)
        {
            if (RunningCount >= MaxConcurrentActions)
            {
                return null;
            }

            var metadata = new ActionMetadata
            {
                Name = actionName,
                State = LoadingState.Loading,
                StartTime = DateTime.Now
            };

            _actions.Add(metadata);
            OnQueueChanged();
            return metadata;
        }
    }

    /// <summary>
    /// Updates the state of an action
    /// </summary>
    public void UpdateActionState(Guid actionId, LoadingState state, string? errorMessage = null)
    {
        lock (_lock)
        {
            var action = _actions.FirstOrDefault(a => a.Id == actionId);
            if (action != null)
            {
                action.State = state;
                if (state == LoadingState.Success || state == LoadingState.Error)
                {
                    action.EndTime = DateTime.Now;
                }
                if (state == LoadingState.Error)
                {
                    action.ErrorMessage = errorMessage;
                }
                OnQueueChanged();
            }
        }
    }

    /// <summary>
    /// Updates the progress of an action
    /// </summary>
    public void UpdateActionProgress(Guid actionId, int progress)
    {
        lock (_lock)
        {
            var action = _actions.FirstOrDefault(a => a.Id == actionId);
            if (action != null)
            {
                action.Progress = Math.Clamp(progress, 0, 100);
                OnQueueChanged();
            }
        }
    }

    /// <summary>
    /// Removes completed actions from the queue
    /// </summary>
    public void CleanupCompletedActions()
    {
        lock (_lock)
        {
            var completedActions = _actions
                .Where(a => a.State == LoadingState.Success || a.State == LoadingState.Error)
                .Where(a => a.EndTime.HasValue && (DateTime.Now - a.EndTime.Value).TotalSeconds > 5)
                .ToList();

            foreach (var action in completedActions)
            {
                _actions.Remove(action);
            }

            if (completedActions.Any())
            {
                OnQueueChanged();
            }
        }
    }

    /// <summary>
    /// Clears all actions from the queue
    /// </summary>
    public void ClearAll()
    {
        lock (_lock)
        {
            _actions.Clear();
            OnQueueChanged();
        }
    }

    private void OnQueueChanged()
    {
        QueueChanged?.Invoke(this, EventArgs.Empty);
    }
}
