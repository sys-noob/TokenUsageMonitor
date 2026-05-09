using System;
using System.Globalization;
using System.Windows.Data;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.Converters;

public class ChartNormalizationConverter : IValueConverter
{
    public double MaxHeight { get; set; } = 60;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ChartDataPoint point && parameter != null)
        {
            int index = parameter switch
            {
                string s => int.TryParse(s, out var parsed) ? parsed : 0,
                int i => i,
                _ => 0
            };

            if (point.Values.Count > index && point.MaxValue > 0)
            {
                return point.Values[index] / point.MaxValue * MaxHeight;
            }
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
