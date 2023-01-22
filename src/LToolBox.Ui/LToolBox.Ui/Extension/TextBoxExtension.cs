using System.Text;
using Avalonia.Controls;

namespace LToolBox.Ui.Extension;

public static class TextBoxExtension
{
    private static readonly StringBuilder _stringBuilder = new();

    public static void MoveCaretToEnd(this TextBox textBox)
    {
        if (string.IsNullOrEmpty(textBox.Text))
            return;

        textBox.CaretIndex = textBox.Text.Length;
    }

    public static void InsertCharAtCaretPos(this TextBox textBox, char value)
    {
        _stringBuilder.Append(textBox.Text);

        if (textBox.CaretIndex > _stringBuilder.Length)
            _stringBuilder.Append(value);
        else
            _stringBuilder.Insert(textBox.CaretIndex, value);

        textBox.Text = _stringBuilder.ToString();
        _stringBuilder.Clear();
    }
}