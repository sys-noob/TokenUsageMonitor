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
            var rawBody = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                LogResponse($"GLM HTTP {(int)response.StatusCode}", rawBody);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    return ErrorResult("API Key 无效");
                return ErrorResult($"HTTP错误: {(int)response.StatusCode}");
            }

            var wrapper = System.Text.Json.JsonSerializer.Deserialize<GlmResponseWrapper>(rawBody);
            if (wrapper?.Data == null)
            {
                LogResponse("GLM 解析失败", rawBody);
                return ErrorResult("响应为空或格式无效");
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
            System.IO.File.AppendAllText(path, $"[{DateTime.Now:HH:mm:ss}] [GLM] {context}: {body}{Environment.NewLine}");
        }
        catch { }
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
