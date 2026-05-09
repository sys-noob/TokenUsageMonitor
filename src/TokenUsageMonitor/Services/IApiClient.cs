using System.Threading;
using System.Threading.Tasks;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.Services;

public interface IApiClient
{
    string PlatformId { get; }
    Task<QuotaInfo> GetQuotaAsync(string apiKey, CancellationToken ct = default);
}
