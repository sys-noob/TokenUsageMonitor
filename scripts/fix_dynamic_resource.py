"""Batch replace StaticResource→DynamicResource for Brush resources in XAML files."""
import re
import sys

BRUSH_KEYS = [
    "MainBackgroundBrush", "CardBackgroundBrush", "PrimaryTextBrush", "SecondaryTextBrush",
    "BorderBrush", "GlmBrandBrush", "KimiBrandBrush", "DeepSeekBrandBrush",
    "ProgressBarBackgroundBrush", "ProgressBarFillBrush",
    "SuccessBrush", "WarningBrush", "ErrorBrush",
    "ChartSeries1Brush", "ChartSeries2Brush", "ChartSeries3Brush", "ChartSeries4Brush", "ChartSeries5Brush",
    "ToolTipBackgroundBrush",
]

# Also match local references like {StaticResource CardBackgroundBrush}
# But NOT x:Key="CardBackgroundBrush" definitions

def replace_in_file(path):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    original = content

    for key in BRUSH_KEYS:
        # Replace {StaticResource KEY} with {DynamicResource KEY}
        # But NOT x:Key="KEY" or x:Key="{StaticResource KEY}"
        pattern = r'\{StaticResource\s+' + re.escape(key) + r'\}'
        replacement = '{DynamicResource ' + key + '}'
        content = re.sub(pattern, replacement, content)

    if content != original:
        with open(path, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f"Updated: {path}")
    else:
        print(f"No changes: {path}")

if __name__ == "__main__":
    for path in sys.argv[1:]:
        replace_in_file(path)
