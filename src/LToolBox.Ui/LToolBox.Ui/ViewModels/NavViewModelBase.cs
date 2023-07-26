namespace LToolBox.Ui.ViewModels;

public abstract class NavViewModelBase : ViewModelBase
{
    public abstract string NavHeader { get; }
    public abstract string? IconKey { get; }
}