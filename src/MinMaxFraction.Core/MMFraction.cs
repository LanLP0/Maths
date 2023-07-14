namespace MinMaxFraction.Core;

//      ax^2 + bx + c
// A = ---------------
//      ex^2 + fx + g

internal sealed class MMFraction
{
    // Top
    public double T0 { get; set; } = 0; // a
    public double T1 { get; set; } = 0; // b
    public double T2 { get; set; } = 0; // c

    // Bottom
    public double B0 { get; set; } = 0; // e
    public double B1 { get; set; } = 0; // f
    public double B2 { get; set; } = 0; // g

    public MMPolynomial Calc()
    {
        return new MMPolynomial
        {
            V0 = (B0, -T0),
            V1 = (B1, -T1),
            V2 = (B2, -T2)
        };
    }

    /// <summary>
    ///     Validate if the fraction is valid
    /// </summary>
    /// <returns>true if nominator and denominator is not 0; false otherwise</returns>
    public bool Validate()
    {
        return T0 != 0 && T1 != 0 && T2 != 0 &&
               B0 != 0 && B1 != 0 && B2 != 0;
    }
}