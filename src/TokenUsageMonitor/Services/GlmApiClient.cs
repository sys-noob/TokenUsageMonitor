using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.Services;

public class GlmApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    public string PlatformId => "GLM";

    public GlmApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QuotaInfo> GetQuotaAsync(string apiKey, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://open.bigmodel.cn/api/paas/v4/user/info");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");

            using var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var wrapper = await response.Content.ReadFromJsonAsync<GlmResponseWrapper>(ct).ConfigureAwait(false);
            if (wrapper?.Data == null)
            {
                return ErrorResult("Empty or invalid response");
            }

            var total = wrapper.Data.TotalQuota;
            var used = wrapper.Data.UsedQuota;

            return new QuotaInfo
            {
                PlatformName = "GLM",
                PlatformId = PlatformId,
                UsedAmount = used,
                TotalAmount = total,
                Unit = "tokens",
                Status = QuotaStatus.Normal,
                LastUpdated = DateTime.Now
            };
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return new QuotaInfo
            {
                PlatformName = "GLM",
                PlatformId = PlatformId,
                Status = QuotaStatus.Timeout,
                ErrorMessage = "Request timed out",
                LastUpdated = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            return ErrorResult(ex.Message);
        }
    }

    private static QuotaInfo ErrorResult(string message)
    {
        return new QuotaInfo
        {
            PlatformName = "GLM",
            PlatformId = "GLM",
            Status = QuotaStatus.Error,
            ErrorMessage = message,
            LastUpdated = DateTime.Now
        };
    }

    private class GlmResponseWrapper
    {
        [JsonPropertyName("data")]
        public GlmData? Data { get; set; }
    }

    private class GlmData
    {
        [JsonPropertyName("total_quota")]
        public double TotalQuota { get; set; }

        [JsonPropertyName("used_quota")]
        public double UsedQuota { get; set; }
    }
}
