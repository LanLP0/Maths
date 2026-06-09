using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace LToolBox.Ui.Utils.Converter;

public sealed class SolidColorBrushToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ISolidColorBrush brush || targetType != typeof(Color))
            return Colors.White;
        
        return ConvertStatic(brush);
    }
    
    public static object? ConvertStatic(object? value)
    {
        return (value as ISolidColorBrush)!.Color;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new UnreachableException();
    }
}