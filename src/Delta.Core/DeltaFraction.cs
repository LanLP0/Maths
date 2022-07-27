namespace Delta.Core;

internal class DeltaFraction
{
    public SimpleVariable T0 { get; } = new();
    public SimpleVariable T1 { get; } = new();
    public SimpleVariable T2 { get; } = new();
    public SimpleVariable B0 { get; } = new();
    public SimpleVariable B1 { get; } = new();
    public SimpleVariable B2 { get; } = new();

    /// <summary>
    ///     Calculate the fraction into a delta, after this, the variables is modified
    /// </summary>
    public Delta Calc()
    {
        T0.NumberPart = -T0.NumberPart;
        T1.NumberPart = -T1.NumberPart;
        T2.NumberPart = -T2.NumberPart;

        return new Delta
        {
            V0 = (B0, T0),
            V1 = (B1, T1),
            V2 = (B2, T2)
        };
    }
}