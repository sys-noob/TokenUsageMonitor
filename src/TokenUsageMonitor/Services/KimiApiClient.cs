using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.Services;

public class KimiApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    public string PlatformId => "KIMI";

    public KimiApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QuotaInfo> GetQuotaAsync(string apiKey, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.moonshot.cn/v1/users/me/balance");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");

            using var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var wrapper = await response.Content.ReadFromJsonAsync<KimiResponseWrapper>(ct).ConfigureAwait(false);
            if (wrapper?.Data == null)
            {
                return ErrorResult("Empty or invalid response");
            }

            var total = wrapper.Data.TotalBalance;
            var available = wrapper.Data.AvailableBalance;

            return new QuotaInfo
            {
                PlatformName = "KIMI",
                PlatformId = PlatformId,
                UsedAmount = total - available,
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
                PlatformName = "KIMI",
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
            PlatformName = "KIMI",
            PlatformId = "KIMI",
            Status = QuotaStatus.Error,
            ErrorMessage = message,
            LastUpdated = DateTime.Now
        };
    }

    private class KimiResponseWrapper
    {
        [JsonPropertyName("data")]
        public KimiData? Data { get; set; }
    }

    private class KimiData
    {
        [JsonPropertyName("available_balance")]
        public double AvailableBalance { get; set; }

        [JsonPropertyName("total_balance")]
        public double TotalBalance { get; set; }
    }
}
