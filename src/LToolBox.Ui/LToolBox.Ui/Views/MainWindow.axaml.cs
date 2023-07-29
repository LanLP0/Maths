using FluentAvalonia.UI.Windowing;

namespace LToolBox.Ui.Views;

public sealed partial class MainWindow : AppWindow
{
    public MainWindow()
    {
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        InitializeComponent();
    }
}