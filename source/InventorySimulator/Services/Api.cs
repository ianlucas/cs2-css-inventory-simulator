/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Ian Lucas. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/

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

    public static string GetUrl(string pathname = "")
    {
        return $"{ConVars.Url.Value}{pathname}";
    }

    public static bool HasApiKey()
    {
        return ConVars.ApiKey.Value != "";
    }

    private static async Task<HttpResponseMessage?> SendPostRequest(string url, object request)
    {
        try
        {
            var content = JsonContent.Create(request);
            var response = await _httpClient.PostAsync(url, content);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                CSS.Plugin.Logger.LogError(
                    "POST {Url} failed, check your invsim_apikey's value.",
                    url
                );
                return null;
            }
            if (!response.IsSuccessStatusCode)
            {
                CSS.Plugin.Logger.LogError(
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
            CSS.Plugin.Logger.LogError("POST {Url} failed: {Message}", url, error.Message);
            return null;
        }
    }

    private static async Task PostAsync(string url, object request)
    {
        await SendPostRequest(url, request);
    }

    private static async Task<T?> PostAsync<T>(string url, object request)
        where T : class
    {
        var response = await SendPostRequest(url, request);
        if (response == null)
            return null;
        var responseContent = await response.Content.ReadAsStringAsync();
        return string.IsNullOrEmpty(responseContent)
            ? null
            : JsonSerializer.Deserialize<T>(responseContent);
    }

    public static async Task<EquippedV4Response?> FetchEquipped(ulong steamId)
    {
        var url = GetUrl($"/api/equipped/v4/{steamId}.json");
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var jsonContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<EquippedV4Response>(jsonContent);
            }
            catch (Exception error)
            {
                CSS.Plugin.Logger.LogError(
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

    public static async Task SendStatTrakIncrement(int targetUid, string userId)
    {
        var url = GetUrl("/api/increment-item-stattrak");
        var request = new StatTrakIncrementRequest
        {
            ApiKey = ConVars.ApiKey.Value,
            TargetUid = targetUid,
            UserId = userId,
        };
        await PostAsync(url, request);
    }

    public static async Task<SignInUserResponse?> SendSignIn(string userId)
    {
        var url = GetUrl("/api/sign-in");
        var request = new SignInRequest { ApiKey = ConVars.ApiKey.Value, UserId = userId };
        return await PostAsync<SignInUserResponse>(url, request);
    }
}
