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
    public double Width { get; set; } = 240;
    public double Height { get; set; } = 44;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<LineChartDataPoint> data) return new PointCollection();

        var points = new PointCollection();
        var list = new List<LineChartDataPoint>(data);
        if (list.Count == 0) return points;

        double stepX = Width / Math.Max(list.Count - 1, 1);

        for (int i = 0; i < list.Count; i++)
        {
            double x = i * stepX;
            double y = Height - (list[i].Value1 / 100.0 * Height);
            points.Add(new System.Windows.Point(x, y));
        }

        return points;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class HealthDataToEllipsePositionsConverter : IValueConverter
{
    public double Width { get; set; } = 240;
    public double Height { get; set; } = 44;
    public double EllipseSize { get; set; } = 4;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<LineChartDataPoint> data) return new PointCollection();
        var points = new PointCollection();
        var list = new List<LineChartDataPoint>(data);
        if (list.Count == 0) return points;

        double stepX = Width / Math.Max(list.Count - 1, 1);
        double offset = EllipseSize / 2;

        for (int i = 0; i < list.Count; i++)
        {
            double x = i * stepX - offset;
            double y = Height - (list[i].Value1 / 100.0 * Height) - offset;
            points.Add(new System.Windows.Point(x, y));
        }

        return points;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
