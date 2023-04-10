namespace Delta.Core;

internal sealed class DeltaFraction
{
    public SimpleVariable N0 { get; } = new();
    public SimpleVariable N1 { get; } = new();
    public SimpleVariable N2 { get; } = new();
    public SimpleVariable D0 { get; } = new();
    public SimpleVariable D1 { get; } = new();
    public SimpleVariable D2 { get; } = new();

    /// <summary>
    ///     Calculate the fraction into a delta, after this, the variables is modified
    /// </summary>
    public Delta Calc()
    {
        N0.NumberPart = -N0.NumberPart;
        N1.NumberPart = -N1.NumberPart;
        N2.NumberPart = -N2.NumberPart;

        return new Delta
        {
            V0 = (D0, N0),
            V1 = (D1, N1),
            V2 = (D2, N2)
        };
    }

    /// <summary>
    ///     Validate if the fraction is valid
    /// </summary>
    /// <returns>true if nominator and denominator is not 0; false otherwise</returns>
    public bool Validate()
    {
        return !((N0.IsZero() && N1.IsZero() && N2.IsZero()) ||
                 (D0.IsZero() && D1.IsZero() && D2.IsZero()));
    }
}