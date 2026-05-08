namespace TokenUsageMonitor.Models;

public class ServiceStatusItem
{
    public string Name { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public bool IsHealthy { get; set; }
}
