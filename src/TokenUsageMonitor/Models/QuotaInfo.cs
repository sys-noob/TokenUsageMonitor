using System;

namespace TokenUsageMonitor.Models;

public class QuotaInfo
{
    public string PlatformName { get; set; } = "";
    public string PlatformId { get; set; } = "";
    public double UsedAmount { get; set; }
    public double TotalAmount { get; set; }
    public string Unit { get; set; } = "tokens";
    public DateTime? ExpiryDate { get; set; }
    public QuotaStatus Status { get; set; }
    public string ErrorMessage { get; set; } = "";
    public DateTime LastUpdated { get; set; }
    public string DisplayPercent => $"{UsedAmount / Math.Max(TotalAmount, 1) * 100:F0}%";
    public double Percentage => UsedAmount / Math.Max(TotalAmount, 1) * 100;
}

public enum QuotaStatus { Normal, Error, Loading, Timeout }
