// ReSharper disable InconsistentNaming

namespace LCalc.Tests.TestCases;

public static partial class TestCases
{
    public static readonly string[] Step_ShouldBe_Expected =
    {
        "1+(1+1)&step",
        "[foo(a b c)=a^(b+c)]foo(2 1 (1+foo(2 1 1))) &step",
        "abs(sin(((~1>>2<<2)^2!/1000*50-40+1))) &step",
        "abs(sin(((~1>>2<<2)^2!/1000*50-40+1))) &tree",
        "abs(1+(2+3))==2+4!= (1+(2+(3+4))) &step",
        "1-|2-(3+(4 &render &step", // Implicit braces
        "sigma(x, 1, 100, -1*x) &latex",
        "cpi(x, floor(sqrt(6)), ceiling(cbrt(999)), x^x) &latex",
        "cpi(x, floor(sqrt(6)), ceiling(cbrt(999)), x^x) &latexdoc",
        "cpi(x, floor(sqrt(6)), ceiling(cbrt(999)), x^x) &render",
        "1+(2+a) &a=10 &step &render"
    };
}