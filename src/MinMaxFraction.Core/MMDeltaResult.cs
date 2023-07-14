using Common.Maths;
using Common.Maths.Extension;

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

    public string RenderResult(bool humanize = true)
    {
        switch (Calc(out var result1, out var result2))
        {
            case -1:
            {
                return "Infinite results";
            }
            case 0:
            {
                return "No result";
            }
            case 1:
            {
                if (humanize)
                    return result1!.Value.Humanize();
                
                return result1!.Value.ToString();
            }
            case 2:
            {
                if (humanize)
                    return $"{result1!.Value.Humanize()}, {result2!.Value.Humanize()}";

                return $"{result1!.Value}, {result2!.Value}";
            }
        }

        // Unreachable
        return string.Empty;
    }
}