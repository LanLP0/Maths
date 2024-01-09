using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Metadata;
using LToolBox.Ui.Extension;

namespace LToolBox.Ui.Controls;

public sealed class SwitchPanel : TemplatedControl
{
    public static readonly StyledProperty<Control> ContentProperty =
        AvaloniaProperty.Register<SwitchPanel, Control>(nameof(Content));

    public Control Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    [Content] public List<Control> Controls { get; } = [];
    
    public int Index { get; private set; }

    protected override void OnInitialized()
    {
        Content = Controls[0];

        Template = new FuncControlTemplate((_, _) => new ContentPresenter
        {
            [!ContentPresenter.ContentProperty] = new Binding(nameof(Content)).WithSource(this)
        });
    }

    public void SetContentIndex(int index)
    {
        if (Index == index)
            return;
        
        Index = index;
        Content = Controls[index];
    }
}