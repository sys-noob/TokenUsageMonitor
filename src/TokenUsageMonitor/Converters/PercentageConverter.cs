using System;
using System.Globalization;
using System.Windows.Data;

namespace TokenUsageMonitor.Converters;

public class PercentageConverter : IValueConverter
{
    public double MaxWidth { get; set; } = 260;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var max = parameter is string s && double.TryParse(s, out var parsed) ? parsed : MaxWidth;
        if (value is double d)
        {
            return d / 100.0 * max;
        }
        return 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
