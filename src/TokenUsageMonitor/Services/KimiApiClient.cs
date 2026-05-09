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
            var rawBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                LogResponse($"KIMI HTTP {(int)response.StatusCode}", rawBody);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return ErrorResult("API Key 无效");
                return ErrorResult($"HTTP错误: {(int)response.StatusCode}");
            }

            var wrapper = System.Text.Json.JsonSerializer.Deserialize<KimiResponseWrapper>(rawBody);
            if (wrapper?.Data == null)
            {
                LogResponse("KIMI 解析失败", rawBody);
                return ErrorResult("响应为空或格式无效");
            }

            var total = wrapper.Data.TotalBalance ?? 0;
            var available = wrapper.Data.AvailableBalance ?? wrapper.Data.Balance ?? 0;

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
                ErrorMessage = "请求超时",
                LastUpdated = DateTime.Now
            };
        }
        catch (Exception ex)
        {
            return ErrorResult(ex.Message);
        }
    }

    private static void LogResponse(string context, string body)
    {
        try
        {
            var path = System.IO.Path.Combine(System.AppContext.BaseDirectory, "error.log");
            System.IO.File.AppendAllText(path, $"[{DateTime.Now:HH:mm:ss}] [KIMI] {context}: {body}{System.Environment.NewLine}");
        }
        catch { }
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
        public double? AvailableBalance { get; set; }

        [JsonPropertyName("balance")]
        public double? Balance { get; set; }

        [JsonPropertyName("total_balance")]
        public double? TotalBalance { get; set; }
    }
}
