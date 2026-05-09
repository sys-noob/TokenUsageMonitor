namespace TokenUsageMonitor.Models;

public class ConcurrentTestResult
{
    public string PlatformName { get; set; } = "";
    public bool Success { get; set; }
    public int LatencyMs { get; set; }
    public string ErrorMessage { get; set; } = "";
}
