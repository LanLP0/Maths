namespace LCalc.Helpers;

internal static class StringExtension
{
    public static bool TryGetValueAt(this string str, int index, out char value)
    {
        if (index < 0 || index >= str.Length)
        {
            value = default;
            return false;
        }

        value = str[index];
        return true;
    }
}