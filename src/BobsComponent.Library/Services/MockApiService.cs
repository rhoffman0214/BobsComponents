using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BobsComponent.Library.Models;

namespace BobsComponent.Library.Services;

/// <summary>
/// Provides simulated and real API operations for testing async actions
/// </summary>
/// <remarks>
/// WARNING: This service exposes exception details in error messages for demo/testing purposes.
/// In production services, NEVER expose exception.Message to clients - log errors server-side
/// and return generic error messages to prevent information disclosure vulnerabilities.
/// </remarks>
public class MockApiService
{
    private readonly HttpClient _httpClient;
    private readonly Random _random = new();

    public MockApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Simulates a quick operation (500ms)
    /// </summary>
    public async Task<AsyncOperationResult> QuickOperationAsync(
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await SimulateDelayAsync(500, progress, cancellationToken);
    }

    /// <summary>
    /// Simulates a medium-length operation (2 seconds)
    /// </summary>
    public async Task<AsyncOperationResult> MediumOperationAsync(
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await SimulateDelayAsync(2000, progress, cancellationToken);
    }

    /// <summary>
    /// Simulates a slow operation (5 seconds)
    /// </summary>
    public async Task<AsyncOperationResult> SlowOperationAsync(
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        return await SimulateDelayAsync(5000, progress, cancellationToken);
    }

    /// <summary>
    /// Fetches a random post from JSONPlaceholder API
    /// </summary>
    public async Task<AsyncOperationResult> FetchPostAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var postId = _random.Next(1, 101);
            var response = await _httpClient.GetFromJsonAsync<object>(
                $"https://jsonplaceholder.typicode.com/posts/{postId}",
                cancellationToken);

            return AsyncOperationResult.CreateSuccess(response);
        }
        catch (Exception ex)
        {
            return AsyncOperationResult.CreateError($"API Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Fetches a random user from JSONPlaceholder API
    /// </summary>
    public async Task<AsyncOperationResult> FetchUserAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _random.Next(1, 11);
            var response = await _httpClient.GetFromJsonAsync<object>(
                $"https://jsonplaceholder.typicode.com/users/{userId}",
                cancellationToken);

            return AsyncOperationResult.CreateSuccess(response);
        }
        catch (Exception ex)
        {
            return AsyncOperationResult.CreateError($"API Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Fetches random todos from JSONPlaceholder API
    /// </summary>
    public async Task<AsyncOperationResult> FetchTodosAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = _random.Next(1, 11);
            var response = await _httpClient.GetFromJsonAsync<object>(
                $"https://jsonplaceholder.typicode.com/users/{userId}/todos",
                cancellationToken);

            return AsyncOperationResult.CreateSuccess(response);
        }
        catch (Exception ex)
        {
            return AsyncOperationResult.CreateError($"API Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Simulates an operation with a configurable error rate
    /// </summary>
    public async Task<AsyncOperationResult> UnreliableOperationAsync(
        double errorRate = 0.3,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(_random.Next(1000, 3000), cancellationToken);

        if (_random.NextDouble() < errorRate)
        {
            return AsyncOperationResult.CreateError("Operation failed randomly");
        }

        return AsyncOperationResult.CreateSuccess();
    }

    /// <summary>
    /// Internal helper to simulate delays with progress reporting
    /// </summary>
    private async Task<AsyncOperationResult> SimulateDelayAsync(
        int totalMilliseconds,
        IProgress<int>? progress,
        CancellationToken cancellationToken)
    {
        try
        {
            const int steps = 10;
            var stepDelay = totalMilliseconds / steps;

            for (int i = 0; i <= steps; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                progress?.Report((i * 100) / steps);

                if (i < steps)
                {
                    await Task.Delay(stepDelay, cancellationToken);
                }
            }

            return AsyncOperationResult.CreateSuccess();
        }
        catch (OperationCanceledException)
        {
            return AsyncOperationResult.CreateError("Operation was cancelled");
        }
    }
}
