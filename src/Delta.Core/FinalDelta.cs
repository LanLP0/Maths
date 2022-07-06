using Common.Maths;
using JetBrains.Annotations;

namespace Delta.Core;

public class FinalDelta
{
    [NotNull]
    public SimpleVariable T0 { get; set; }
    [NotNull]
    public SimpleVariable T1 { get; set; }
    [NotNull]
    public SimpleVariable T2 { get; set; }

    public int Calc(out double? result1, out double? result2) =>
        Polynomial.Calc2(T0.NumberPart, T1.NumberPart, T2.NumberPart, out result1, out result2);
}