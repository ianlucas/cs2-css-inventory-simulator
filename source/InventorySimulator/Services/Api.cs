/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace InventorySimulator;

public class Api
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

    private const int MaxRetries = 3;

    private const int RetryDelayMs = 100;

    // Mirrors the rate limits enforced by Inventory Simulator's public API.
    private const double StatTrakIncrementRateLimitCapacity = 50;
    private const double StatTrakIncrementRateLimitRefillIntervalSeconds = 3.6;
    private const double SprayConsumeRateLimitCapacity = 1;
    private const double SprayConsumeRateLimitRefillIntervalSeconds = 30;

    private static volatile bool _isSuspended = false;

    private static readonly ConcurrentDictionary<(ulong, int), RateLimitBucket> _statTrakBuckets =
        new();

    private static readonly ConcurrentDictionary<(ulong, int), RateLimitBucket> _sprayBuckets =
        new();

    private class RateLimitBucket(double capacity, double refillIntervalSeconds)
    {
        private readonly double _capacity = capacity;
        private double _tokens = capacity;
        private DateTime _updatedAt = DateTime.UtcNow;

        public bool TryConsume()
        {
            lock (this)
            {
                var now = DateTime.UtcNow;
                var elapsedSeconds = (now - _updatedAt).TotalSeconds;
                _tokens = Math.Min(_capacity, _tokens + elapsedSeconds / refillIntervalSeconds);
                _updatedAt = now;
                if (_tokens < 1)
                    return false;
                _tokens -= 1;
                return true;
            }
        }
    }

    public static void ResetSuspension()
    {
        _isSuspended = false;
    }

    public static string GetUrl(string pathname = "")
    {
        return $"{ConVars.Url.Value}{pathname}";
    }

    public static bool HasApiKey()
    {
        return ConVars.ApiKey.Value != "";
    }

    private static string? GetApiKeyOrNull()
    {
        return HasApiKey() ? ConVars.ApiKey.Value : null;
    }

    private static async Task<HttpResponseMessage?> SendPostAsync(
        string url,
        object request,
        bool suspendOnUnauthorized = false
    )
    {
        try
        {
            var content = JsonContent.Create(request);
            var response = await _httpClient.PostAsync(url, content);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                if (suspendOnUnauthorized)
                    _isSuspended = true;
                Runtime.Plugin.Logger.LogError(
                    "POST {Url} failed, check your invsim_apikey's value.",
                    url
                );
                return null;
            }
            if (!response.IsSuccessStatusCode)
            {
                Runtime.Plugin.Logger.LogError(
                    "POST {Url} failed with status code: {StatusCode}",
                    url,
                    response.StatusCode
                );
                return null;
            }
            return response;
        }
        catch (Exception error)
        {
            Runtime.Plugin.Logger.LogError("POST {Url} failed: {Message}", url, error.Message);
            return null;
        }
    }

    private static async Task PostAsync(string url, object request)
    {
        await SendPostAsync(url, request);
    }

    private static bool CanSendPublicApiRequest(
        bool isEnabled,
        ConcurrentDictionary<(ulong, int), RateLimitBucket> buckets,
        ulong userId,
        int targetUid,
        double capacity,
        double refillIntervalSeconds
    )
    {
        if (HasApiKey())
            return true;
        if (!isEnabled)
            return false;
        var bucket = buckets.GetOrAdd(
            (userId, targetUid),
            _ => new RateLimitBucket(capacity, refillIntervalSeconds)
        );
        return bucket.TryConsume();
    }

    private static async Task<T?> PostAsync<T>(string url, object request)
        where T : class
    {
        var response = await SendPostAsync(url, request);
        if (response == null)
            return null;
        var responseContent = await response.Content.ReadAsStringAsync();
        return string.IsNullOrEmpty(responseContent)
            ? null
            : JsonSerializer.Deserialize<T>(responseContent);
    }

    public static async Task<EquippedV5Response?> FetchEquippedAsync(ulong steamId)
    {
        var url = GetUrl($"/api/equipped/v5/{steamId}.json");
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var jsonContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<EquippedV5Response>(jsonContent);
            }
            catch (Exception error)
            {
                Runtime.Plugin.Logger.LogError(
                    "GET {Url} failed (attempt {Attempt}/{MaxRetries}): {Message}",
                    url,
                    attempt,
                    MaxRetries,
                    error.Message
                );
                if (attempt == MaxRetries)
                    return null;
                await Task.Delay(TimeSpan.FromMilliseconds(RetryDelayMs * attempt));
            }
        return null;
    }

    public static async Task SendStatTrakIncrementAsync(ulong userId, int targetUid)
    {
        if (_isSuspended)
            return;
        if (
            !CanSendPublicApiRequest(
                ConVars.IsPublicApiStatTrakIncrement.Value,
                _statTrakBuckets,
                userId,
                targetUid,
                StatTrakIncrementRateLimitCapacity,
                StatTrakIncrementRateLimitRefillIntervalSeconds
            )
        )
            return;
        var url = GetUrl("/api/increment-item-stattrak");
        var request = new StatTrakIncrementRequest
        {
            ApiKey = GetApiKeyOrNull(),
            TargetUid = targetUid,
            UserId = userId.ToString(),
        };
        await SendPostAsync(url, request, suspendOnUnauthorized: true);
    }

    public static async void SendStatTrakIncrement(ulong userId, int targetUid)
    {
        await SendStatTrakIncrementAsync(userId, targetUid);
    }

    public static async Task SendConsumeItemSprayAsync(ulong userId, int targetUid)
    {
        if (_isSuspended)
            return;
        if (
            !CanSendPublicApiRequest(
                ConVars.IsPublicApiSprayConsume.Value,
                _sprayBuckets,
                userId,
                targetUid,
                SprayConsumeRateLimitCapacity,
                SprayConsumeRateLimitRefillIntervalSeconds
            )
        )
            return;
        var url = GetUrl("/api/consume-item-spray");
        var request = new ConsumeItemSprayRequest
        {
            ApiKey = GetApiKeyOrNull(),
            TargetUid = targetUid,
            UserId = userId.ToString(),
        };
        await SendPostAsync(url, request, suspendOnUnauthorized: true);
    }

    public static async void SendConsumeItemSpray(ulong userId, int targetUid)
    {
        await SendConsumeItemSprayAsync(userId, targetUid);
    }

    public static async Task<SignInUserResponse?> SendSignIn(string userId)
    {
        var url = GetUrl("/api/sign-in");
        var request = new SignInRequest { ApiKey = ConVars.ApiKey.Value, UserId = userId };
        return await PostAsync<SignInUserResponse>(url, request);
    }
}
