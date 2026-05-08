using System.Windows;
using System.Windows.Controls;
using TokenUsageMonitor.Models;

namespace TokenUsageMonitor.Views;

public class PlatformTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ZhipuTemplate { get; set; }
    public DataTemplate? MiniMaxTemplate { get; set; }
    public DataTemplate? DeepSeekTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is PlatformInfo platform)
        {
            return platform.Name switch
            {
                "智谱" => ZhipuTemplate,
                "MiniMax" => MiniMaxTemplate,
                "DeepSeek" => DeepSeekTemplate,
                _ => ZhipuTemplate
            };
        }
        return ZhipuTemplate;
    }
}
