using System.Text;

namespace LCalc.Extension;

internal static class StringBuilderExtension
{
    public static bool TryGetValueAt(this StringBuilder stringBuilder, int index, out char value)
    {
        if (index < 0 || index >= stringBuilder.Length)
        {
            value = default;
            return false;
        }

        value = stringBuilder[index];
        return true;
    }

    public static bool IsEmpty(this StringBuilder stringBuilder)
    {
        return stringBuilder.Length is 0;
    }
}