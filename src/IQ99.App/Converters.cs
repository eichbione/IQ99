using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace IQ99.App;

public sealed class StringToSymbolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string key
            && Enum.TryParse<SymbolRegular>(key, ignoreCase: true, out var symbol))
        {
            return symbol;
        }

        return SymbolRegular.Info24;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class BytesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is long bytes ? Core.Formatter.FormatBytes(bytes) : "0 B";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public sealed class HexToBrushConverter : IValueConverter
{
    private static readonly Brush Fallback = new SolidColorBrush(Color.FromRgb(91, 95, 102));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hex
            && TryParseHex(hex, out var color))
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        return Fallback;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    private static bool TryParseHex(string hex, out Color color)
    {
        color = default;
        var raw = hex.TrimStart('#');
        if (raw.Length != 6 && raw.Length != 8)
        {
            return false;
        }

        try
        {
            var r = byte.Parse(raw.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(raw.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(raw.Substring(4, 2), NumberStyles.HexNumber);
            var a = raw.Length == 8 ? byte.Parse(raw.Substring(6, 2), NumberStyles.HexNumber) : byte.MaxValue;
            color = Color.FromArgb(a, r, g, b);
            return true;
        }
        catch
        {
            return false;
        }
    }
}