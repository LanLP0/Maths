using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;

namespace LToolBox.Ui.Controls;

public sealed class SwitchPanel : TemplatedControl
{
    public static readonly StyledProperty<Control?> ContentProperty =
        AvaloniaProperty.Register<SwitchPanel, Control?>(nameof(Content));

    public Control? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    [Content] public List<Control> Controls { get; set; } = new();

    protected override void OnInitialized()
    {
        DataContext = this;
        Content = Controls.Count > 0 ? Controls[0] : null;

        Template = new FuncControlTemplate((_, _) => new ContentPresenter
        {
            [!ContentPresenter.ContentProperty] = new Binding(nameof(Content))
        });
    }

    public void SetContentIndex(int index)
    {
        Content = Controls[index];
    }
}