using System.Windows.Media;

namespace TokenUsageMonitor.Models;

public class PlatformInfo
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public System.Windows.Media.Brush BrandBrush { get; set; } = System.Windows.Media.Brushes.Gray;
    public bool IsPro { get; set; }
}
