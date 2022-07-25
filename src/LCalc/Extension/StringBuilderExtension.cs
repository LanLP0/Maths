using System.Text;

namespace LCalc.Helpers;

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
}