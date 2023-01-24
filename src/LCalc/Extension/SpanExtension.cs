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

        return -1;
    }

    public static int IndexOf(this Span<char> span, char value, int startIndex = 0)
    {
        for (; startIndex < span.Length; startIndex++)
        {
            var chr = span[startIndex];

            if (chr == value)
                return startIndex;
        }

        return -1;
    }
    
    public static Span<char> TrimEnd(this Span<char> span, char trimChar)
    {
        int end = span.Length - 1;
        for (; end >= 0; end--)
        {
            if (span[end] != trimChar)
            {
                break;
            }
        }
 
        return span.Slice(0, end + 1);
    }
}