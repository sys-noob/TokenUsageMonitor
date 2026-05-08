namespace TokenUsageMonitor.Models;

public class UsageItem
{
    public string Title { get; set; } = string.Empty;
    public string SubTitle { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public string UsedText { get; set; } = string.Empty;
    public string TotalText { get; set; } = string.Empty;
    public string TimeRangeText { get; set; } = string.Empty;
    public string DetailText { get; set; } = string.Empty;
}
