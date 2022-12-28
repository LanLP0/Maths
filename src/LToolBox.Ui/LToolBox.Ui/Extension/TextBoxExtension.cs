using Avalonia.Controls;

namespace LToolBox.Ui.Extension;

public static class TextBoxExtension
{
    public static void MoveCaretToEnd(this TextBox textBox)
    {
        if (string.IsNullOrEmpty(textBox.Text))
            return;

        textBox.CaretIndex = textBox.Text.Length;
    }
}