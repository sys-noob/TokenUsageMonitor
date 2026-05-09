using System;
using System.Collections.Generic;

namespace TokenUsageMonitor.Models;

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public List<double> Values { get; set; } = new();
    public List<string> SeriesNames { get; set; } = new();
    public double MaxValue { get; set; }
    public bool ShowLabel { get; set; }
}

public class LineChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public double Value1 { get; set; }
    public double Value2 { get; set; }
}
