namespace Delta.Core;

internal class SimpleVariable
{
    public int NumberPart { get; set; } = 0;

    // public int Power { get; set; } = 0;

    public bool IsZero()
    {
        return NumberPart is 0;
    }
}