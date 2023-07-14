using Common.Maths;

namespace MinMaxFraction.Core;

internal sealed class MMDeltaResult
{
    // Value
    public double V0 { get; init; }
    public double V1 { get; init; }
    public double V2 { get; init; }

    public int Calc(out double? result1, out double? result2)
    {
        return Polynomial.Calc2(V0, V1, V2, out result1, out result2);
    }
}