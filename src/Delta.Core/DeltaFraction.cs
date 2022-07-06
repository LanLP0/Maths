namespace Delta.Core;

public class DeltaFraction
{
    public SimpleVariable T0 { get; } = new() {Power = 2};
    public SimpleVariable T1 { get; } = new() {Power = 1};
    public SimpleVariable T2 { get; } = new() {Power = 0};
    public SimpleVariable B0 { get; } = new() {Power = 2};
    public SimpleVariable B1 { get; } = new() {Power = 1};
    public SimpleVariable B2 { get; } = new() {Power = 0};

    /// <summary>
    ///     Calculate the fraction into a delta, after this, the variables is modified
    /// </summary>
    public Delta Calc()
    {
        T0.NumberPart = -T0.NumberPart;
        T1.NumberPart = -T1.NumberPart;
        T2.NumberPart = -T2.NumberPart;

        B0.PowerOfA = 1;
        B1.PowerOfA = 1;
        B2.PowerOfA = 1;

        return new Delta
        {
            V0 = ( B0, T0 ),
            V1 = ( B1, T1 ),
            V2 = ( B2, T2 )
        };
    }
}