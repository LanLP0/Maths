using System;
using Avalonia;

namespace LToolBox.Ui.Extension;

public static class ThicknessExtension
{
    /// <summary>
    /// Change a single value
    /// </summary>
    /// <param name="thickness">The thickness to change</param>
    /// <param name="value">The value to change to</param>
    /// <param name="side">left-top-right-bottom</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static Thickness ChangeSingle(this Thickness thickness, double value, int side)
    {
        thickness.Deconstruct(out var left, out var top, out var right, out var bottom);
        
        switch (side)
        {
            case 1:
                left = value;
                break;
            case 2:
                top = value;
                break;
            case 3:
                right = value;
                break;
            case 4:
                bottom = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(side), side, "Side must be between 1 and 4");
        }
        
        return new Thickness(left, top, right, bottom);
    }
}