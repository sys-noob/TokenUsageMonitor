using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.Services;

public class DeepSeekApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    public string PlatformId => "DeepSeek";

    public DeepSeekApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<QuotaInfo> GetQuotaAsync(string apiKey, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.deepseek.com/user/balance");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {apiKey}");

            using var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<DeepSeekBalanceResponse>(ct).ConfigureAwait(false);
            if (dto == null)
            {
                return ErrorResult("Empty response");
            }

            var total = ParseDouble(dto.TotalBalance);
            var balance = ParseDouble(dto.Balance);

            return new QuotaInfo
            {
                PlatformName = "DeepSeek",
                PlatformId = PlatformId,
                UsedAmount = total - balance,
                TotalAmount = total,
                Unit = dto.Currency?.ToLowerInvariant() ?? "cny",
                Status = QuotaStatus.Normal,
                LastUpdated = DateTime.Now
            };
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return new QuotaInfo
            {
                PlatformName = "DeepSeek",
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
        finally
        {
            sw.Stop();
        }
    }

    private static QuotaInfo ErrorResult(string message)
    {
        return new QuotaInfo
        {
            PlatformName = "DeepSeek",
            PlatformId = "DeepSeek",
            Status = QuotaStatus.Error,
            ErrorMessage = message,
            LastUpdated = DateTime.Now
        };
    }

    private static double ParseDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;
        return double.TryParse(value, out var result) ? result : 0;
    }

    private class DeepSeekBalanceResponse
    {
        [JsonPropertyName("balance")]
        public string? Balance { get; set; }

        [JsonPropertyName("total_balance")]
        public string? TotalBalance { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }
    }
}
