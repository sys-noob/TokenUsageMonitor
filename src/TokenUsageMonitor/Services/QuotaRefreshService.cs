using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.Services;

public class QuotaRefreshService
{
    private readonly HttpClient _httpClient;
    private readonly List<IApiClient> _clients;

    public QuotaRefreshService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? SharedHttpClient.Instance;
        _clients = new List<IApiClient>
        {
            new GlmApiClient(_httpClient),
            new KimiApiClient(_httpClient),
            new DeepSeekApiClient(_httpClient)
        };
    }

    public async Task<Dictionary<string, QuotaInfo>> RefreshAllAsync(
        Dictionary<string, string> apiKeys,
        CancellationToken ct = default)
    {
        var tasks = _clients.Select(async client =>
        {
            if (!apiKeys.TryGetValue(client.PlatformId, out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
            {
                return new QuotaInfo
                {
                    PlatformName = client.PlatformId,
                    PlatformId = client.PlatformId,
                    Status = QuotaStatus.Error,
                    ErrorMessage = "API key not configured",
                    LastUpdated = DateTime.Now
                };
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            try
            {
                var result = await client.GetQuotaAsync(apiKey, cts.Token).ConfigureAwait(false);
                return result;
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested && !ct.IsCancellationRequested)
            {
                return new QuotaInfo
                {
                    PlatformName = client.PlatformId,
                    PlatformId = client.PlatformId,
                    Status = QuotaStatus.Timeout,
                    ErrorMessage = "Request timed out after 10 seconds",
                    LastUpdated = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                return new QuotaInfo
                {
                    PlatformName = client.PlatformId,
                    PlatformId = client.PlatformId,
                    Status = QuotaStatus.Error,
                    ErrorMessage = ex.Message,
                    LastUpdated = DateTime.Now
                };
            }
        });

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.ToDictionary(r => r.PlatformId, StringComparer.OrdinalIgnoreCase);
    }
}

public static class SharedHttpClient
{
    private static readonly HttpClient _instance;

    static SharedHttpClient()
    {
        _instance = new HttpClient();
    }

    public static HttpClient Instance => _instance;
}
