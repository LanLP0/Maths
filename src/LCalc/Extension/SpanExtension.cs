namespace LCalc.Extension;

public static class SpanExtension
{
    public static bool TryGetValueAt(this ReadOnlySpan<char> span, int index, out char value)
    {
        if (index < 0 || index >= span.Length)
        {
            value = default;
            return false;
        }

        value = span[index];
        return true;
    }

    public static int IndexOf(this ReadOnlySpan<char> span, char value, int startIndex = 0)
    {
        for (; startIndex < span.Length; startIndex++)
        {
            var chr = span[startIndex];

            if (chr == value)
                return startIndex;
        }

        return default;
    }
}