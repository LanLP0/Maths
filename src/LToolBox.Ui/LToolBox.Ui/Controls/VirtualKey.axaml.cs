using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;

namespace LToolBox.Ui.Controls;

public class VirtualKey : TemplatedControl
{
    public static readonly StyledProperty<string> KeyProperty =
        AvaloniaProperty.Register<VirtualKey, string>(nameof(Key));

    public static readonly StyledProperty<double> BoxSizeProperty =
        AvaloniaProperty.Register<VirtualKey, double>(nameof(BoxSize));

    public static readonly StyledProperty<IBrush?> ButtonBackgroundProperty =
        AvaloniaProperty.Register<VirtualKey, IBrush?>(nameof(ButtonBackground));

    [Content]
    public string Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }
    
    public double BoxSize
    {
        get => GetValue(BoxSizeProperty);
        private set => SetValue(BoxSizeProperty, value);
    }

    public IBrush? ButtonBackground
    {
        get => GetValue(ButtonBackgroundProperty);
        set => SetValue(ButtonBackgroundProperty, value);
    }

    public static readonly StyledProperty<IconSource> IconSourceProperty =
        AvaloniaProperty.Register<VirtualKey, IconSource>(nameof(IconSource));

    public IconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public Viewbox Inner { get; private set; }

    private VirtualKeyboard? _keyboard;

    protected override void OnInitialized()
    {
        DataContext = this;
        Focusable = false;

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
                CornerRadius = new CornerRadius(int.MaxValue),
                Padding = new Thickness(20),
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                [!WidthProperty] = new Binding("BoxSize"),
                [!HeightProperty] = new Binding("BoxSize"),
                [!ContentControl.ContentProperty] = new Binding("Inner")
            };

            if (ButtonBackground is not null)
            {
                button[!BackgroundProperty] = new Binding("ButtonBackground");
            }

            if (_keyboard is not null)
            {
                button.Click += (_, _) => _keyboard.KeyClicked.OnNext(Key);
            }

            return button;
        });

        base.OnInitialized();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        BoxSize = Math.Min(e.NewSize.Height, e.NewSize.Width);
        
        base.OnSizeChanged(e);
    }
}