using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using LToolBox.Ui.Extension;

namespace LToolBox.Ui.Controls;

public class VirtualKey : TemplatedControl
{
    public static readonly StyledProperty<string> KeyProperty =
        AvaloniaProperty.Register<VirtualKey, string>(nameof(Key));

    // public static readonly StyledProperty<double> BoxSizeProperty =
    //     AvaloniaProperty.Register<VirtualKey, double>(nameof(BoxSize));

    public static readonly StyledProperty<IBrush?> ButtonBackgroundProperty =
        AvaloniaProperty.Register<VirtualKey, IBrush?>(nameof(ButtonBackground));

    public static readonly StyledProperty<IconSource> IconSourceProperty =
        AvaloniaProperty.Register<VirtualKey, IconSource>(nameof(IconSource));

    private VirtualKeyboard? _keyboard;

    [Content]
    public string Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    // public double BoxSize
    // {
    //     get => GetValue(BoxSizeProperty);
    //     private set => SetValue(BoxSizeProperty, value);
    // }

    public IBrush? ButtonBackground
    {
        get => GetValue(ButtonBackgroundProperty);
        set => SetValue(ButtonBackgroundProperty, value);
    }

    public IconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public Viewbox Inner { get; private set; }

    public VirtualKey()
    {
        BorderThickness = new Thickness(1);
    }

    protected override void OnInitialized()
    {
        Focusable = false;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        VerticalAlignment = VerticalAlignment.Stretch;

        _keyboard = this.FindAncestorOfType<VirtualKeyboard>();

        Inner = new Viewbox
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new IconSourceElement
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IconSource = IconSource
            }
        };

        Template = new FuncControlTemplate((_, _) =>
        {
            var button = new Button
            {
                Focusable = false,
                CornerRadius = new CornerRadius(0),
                // BorderBrush = Brushes.Transparent,
                Padding = new Thickness(20),
                Margin = new Thickness(0),
                // Background = new SolidColorBrush(Colors.Gray, 0.1),
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                // [!WidthProperty] = new Binding(nameof(Width)).WithSource(this),
                // [!HeightProperty] = new Binding(nameof(Height)).WithSource(this),
                [!ContentControl.ContentProperty] = new Binding(nameof(Inner)).WithSource(this)
            };

            if (ButtonBackground is not null)
                button[!BackgroundProperty] = new Binding(nameof(ButtonBackground)).WithSource(this);

            if (_keyboard is not null)
                button.Click += (_, _) => _keyboard.KeyClicked.OnNext(Key);

            return button;
        });

        base.OnInitialized();
    }

    // protected override void OnSizeChanged(SizeChangedEventArgs e)
    // {
    //     BoxSize = Math.Min(e.NewSize.Height, e.NewSize.Width);
    //
    //     base.OnSizeChanged(e);
    // }
}