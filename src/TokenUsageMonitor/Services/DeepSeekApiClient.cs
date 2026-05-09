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
            var rawBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                LogResponse($"DeepSeek HTTP {(int)response.StatusCode}", rawBody);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return ErrorResult("API Key 无效");
                return ErrorResult($"HTTP错误: {(int)response.StatusCode}");
            }

            var dto = System.Text.Json.JsonSerializer.Deserialize<DeepSeekBalanceResponse>(rawBody);
            if (dto == null)
            {
                LogResponse("DeepSeek 解析失败", rawBody);
                return ErrorResult("响应为空或格式无效");
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
                ErrorMessage = "请求超时",
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

    private static void LogResponse(string context, string body)
    {
        try
        {
            var path = System.IO.Path.Combine(System.AppContext.BaseDirectory, "error.log");
            System.IO.File.AppendAllText(path, $"[{DateTime.Now:HH:mm:ss}] [DeepSeek] {context}: {body}{System.Environment.NewLine}");
        }
        catch { }
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
