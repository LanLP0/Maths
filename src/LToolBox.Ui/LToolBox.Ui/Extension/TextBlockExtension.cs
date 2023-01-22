using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Media;

namespace LToolBox.Ui.Extension;

public static class TextBlockExtension
{
    /// <summary>
    ///     Fit a <see cref="TextBlock" />'s content to it viewport
    /// </summary>
    /// <param name="textBlock">The text block to adjust font size</param>
    /// <param name="minFontSize">The minimum font size. default: 1</param>
    /// <param name="maxFontSize">The minimum font size. default: <see cref="TextBlock.FontSize" /></param>
    /// <exception cref="ArgumentException"></exception>
    public static void FitContent(this TextBlock textBlock, double maxFontSize, double minFontSize = 1)
    {
        if (minFontSize < 0)
            throw new ArgumentException($"{nameof(minFontSize)} cannot be less than 1", nameof(minFontSize));

        if (maxFontSize < minFontSize)
            throw new ArgumentException($"{nameof(maxFontSize)} cannot be less than or equal to {nameof(minFontSize)}",
                nameof(maxFontSize));

        if (string.IsNullOrWhiteSpace(textBlock.Text)) // Ignore if content is empty
            return;

        var typeface = new Typeface(
            textBlock.FontFamily,
            textBlock.FontStyle,
            textBlock.FontWeight,
            textBlock.FontStretch);

        var fmtText = new FormattedText(
            textBlock.Text,
            CultureInfo.CurrentCulture,
            FlowDirection.RightToLeft,
            typeface,
            maxFontSize,
            textBlock.Foreground); // The rendered text
        fmtText.Trimming = TextTrimming.None;

        var currSize = textBlock.DesiredSize;
        fmtText.MaxTextWidth = currSize.Width;

        // If text dont fit in TextBlock, decrease the font size
        var decreaseAmount = (maxFontSize - minFontSize) / 10;
        var fontSize = maxFontSize;
        while (fontSize >= minFontSize && fmtText.Height > currSize.Height)
        {
            fontSize -= decreaseAmount;
            fmtText.SetFontSize(fontSize);
        }

        textBlock.FontSize = fontSize;
    }
}