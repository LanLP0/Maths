namespace Common;

internal readonly struct SpanSegment<T>
{
    public SpanSegment(int start, int length)
    {
        Start = start;
        Length = length;
    }

    public int Start { get; }
    public int Length { get; }

    public T GetFirst(ReadOnlySpan<T> s)
    {
        return s[Start];
    }

    public ReadOnlySpan<T> GetSpan(ReadOnlySpan<T> s)
    {
        return s.Slice(Start, Length);
    }

    public Span<T> GetSpan(Span<T> s)
    {
        return s.Slice(Start, Length);
    }
}