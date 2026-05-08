using System.Windows;
using System.Windows.Controls;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.Views;

public class PlatformTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ZhipuTemplate { get; set; }
    public DataTemplate? KimiTemplate { get; set; }
    public DataTemplate? DeepSeekTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is PlatformInfo platform)
        {
            return platform.Name switch
            {
                "GLM" => ZhipuTemplate,
                "KIMI" => KimiTemplate,
                "DeepSeek" => DeepSeekTemplate,
                _ => ZhipuTemplate
            };
        }
        return ZhipuTemplate;
    }
}
