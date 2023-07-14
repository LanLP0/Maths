namespace MinMaxFraction.Core;

//      ax^2 + bx + c
// A = ---------------
//      ex^2 + fx + g
//
// This stage:
// (eA - a)x^2 + (fA - b)x + (gA - c)
// V0^^^^^^      V1^^^^^^    V0^^^^^^

internal sealed class MMPolynomial
{
    // Value
    public (double APart, double NumPart) V0 { get; init; }
    public (double APart, double NumPart) V1 { get; init; }
    public (double APart, double NumPart) V2 { get; init; }

    public MMDeltaResult Calc()
    {
        // Exponent level (of A) (tmp)
        var l0 = 0.0;
        var l1 = 0.0;
        var l2 = 0.0;

        // V1^2 (b^2)
        l0 += Math.Pow(V1.NumPart, 2);
        l1 += V1.APart * V1.NumPart * 2;
        l2 += Math.Pow(V1.APart, 2);

        // -4V0*V2 (-4ac)
        l2 += V0.APart * V2.APart * -4;
        l1 += V0.APart * V2.NumPart * -4;
        l1 += V0.NumPart * V2.APart * -4;
        l0 += V0.NumPart * V2.NumPart * -4;

        return new MMDeltaResult
        {
            V0 = l2,
            V1 = l1,
            V2 = l0
        };
    }
}