using Avalonia.Data;

namespace LToolBox.Ui.Utils.Extension;

public static class BindingExtension
{
    public static Binding WithSource(this Binding binding, object? source)
    {
        binding.Source = source;
        return binding;
    }
}