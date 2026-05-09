using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace TokenUsageMonitor.Converters;

/// <summary>
/// Provides an attached property <see cref="SmoothWidthProperty"/> that animates
/// the <see cref="FrameworkElement.Width"/> property with a 300ms CubicEase transition.
/// </summary>
/// <remarks>
/// Usage in XAML:
/// <Rectangle local:AnimatedWidthConverter.SmoothWidth="{Binding Percentage, Converter={StaticResource PercentageConverter}}" />
/// </remarks>
public static class AnimatedWidthConverter
{
    public static readonly DependencyProperty SmoothWidthProperty =
        DependencyProperty.RegisterAttached(
            "SmoothWidth",
            typeof(double),
            typeof(AnimatedWidthConverter),
            new PropertyMetadata(double.NaN, OnSmoothWidthChanged));

    public static double GetSmoothWidth(FrameworkElement element)
        => (double)element.GetValue(SmoothWidthProperty);

    public static void SetSmoothWidth(FrameworkElement element, double value)
        => element.SetValue(SmoothWidthProperty, value);

    private static void OnSmoothWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        if (e.NewValue is not double newValue || double.IsNaN(newValue))
            return;

        if (!element.IsLoaded)
        {
            element.Width = newValue;
            return;
        }

        var animation = new DoubleAnimation
        {
            To = newValue,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        element.BeginAnimation(FrameworkElement.WidthProperty, animation);
    }
}
