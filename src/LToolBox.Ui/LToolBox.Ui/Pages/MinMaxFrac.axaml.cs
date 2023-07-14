using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LToolBox.Ui.Pages;

public partial class MinMaxFrac : UserControl
{
    public MinMaxFrac()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}