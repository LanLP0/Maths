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

namespace LToolBox.Ui.Controls;

public class VirtualKey : TemplatedControl
{
    public static readonly StyledProperty<string> KeyProperty =
        AvaloniaProperty.Register<VirtualKey, string>(nameof(Key));

    public static readonly StyledProperty<IBrush?> ButtonBackgroundProperty =
        AvaloniaProperty.Register<VirtualKey, IBrush?>(nameof(ButtonBackground));

    public static readonly StyledProperty<FAIconSource> IconSourceProperty =
        AvaloniaProperty.Register<VirtualKey, FAIconSource>(nameof(IconSource));

    private VirtualKeyboard? _keyboard;

    [Content]
    public string Key
    {
        get => GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public IBrush? ButtonBackground
    {
        get => GetValue(ButtonBackgroundProperty);
        set => SetValue(ButtonBackgroundProperty, value);
    }

    public FAIconSource IconSource
    {
        get => GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public Viewbox Inner { get; private set; }

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
            Child = new FAIconSourceElement
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
                Padding = new Thickness(20),
                Margin = new Thickness(0),
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                [!ContentControl.ContentProperty] = CompiledBinding.Create((VirtualKey vk) => vk.Inner, this)
            };

            if (ButtonBackground is not null)
                button[!BackgroundProperty] = CompiledBinding.Create((VirtualKey vk) => vk.Inner, this);

            if (_keyboard is not null)
                button.Click += (_, _) => _keyboard.KeyClicked.OnNext(Key);

            return button;
        });

        base.OnInitialized();
    }
}