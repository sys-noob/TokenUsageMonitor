using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.Converters;

public class HealthDataToPointsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<LineChartDataPoint> data) return new PointCollection();

        var points = new PointCollection();
        var list = new List<LineChartDataPoint>(data);
        if (list.Count == 0) return points;

        double width = 240;
        double height = 44;
        double stepX = width / Math.Max(list.Count - 1, 1);

        for (int i = 0; i < list.Count; i++)
        {
            double x = i * stepX;
            double y = height - (list[i].Value1 / 100.0 * height);
            points.Add(new System.Windows.Point(x, y));
        }

        return points;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
