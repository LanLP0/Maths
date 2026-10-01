using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LToolBox.Ui.Converters;

public sealed class ColorConverters : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ISolidColorBrush brush)
            return brush.Color;
        
        throw new NotImplementedException();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}