using Common.Maths;

namespace Delta.Core;

internal sealed class FinalDelta
{
    public SimpleVariable T0 { get; init; } = null!;
    public SimpleVariable T1 { get; init; } = null!;
    public SimpleVariable T2 { get; init; } = null!;

    public int Calc(out double? result1, out double? result2)
    {
        return Polynomial.Calc2(T0.NumberPart, T1.NumberPart, T2.NumberPart, out result1, out result2);
    }
}