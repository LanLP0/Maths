// ReSharper disable InconsistentNaming

namespace LCalc.Tests.TestCases;

public static partial class TestCases
{
    public static readonly string[] Render_ShouldBe_Expected =
    {
        "1+(1+1)&step",
        "[foo(a b c)=a^(b+c)]foo(2 1 (1+foo(2 1 1))) &step",
        "abs(sin(((~1>>2<<2)^2!/1000*50-40+1))) &step",
        "abs(sin(((~1>>2<<2)^2!/1000*50-40+1))) &tree",
        "abs(1+(2+3))==2+4!= (1+(2+(3+4))) &step",
        // Format
        "|1/3|+|1/8|+|1/143| &step",
        "|1/3|+|1/8|+|1/143| &step &raw",
        // Implicit braces
        "1-|2-(3+(4 &render &step",
        "sigma(x, 1, 100, -1*x) &latex",
        "cpi(x, floor(sqrt(6)), ceiling(cbrt(999)), x^x) &latex",
        "cpi(x, floor(sqrt(6)), ceiling(cbrt(999)), x^x) &latexdoc",
        "cpi(x, floor(sqrt(6)), ceiling(cbrt(999)), x^x) &render",
        "sigma(x, 1, 100, x^2-2*x+1) &step",
        // Implicit variable
        "sigma(1, 100, -1*x) &latex",
        "cpi(floor(sqrt(6)), ceiling(cbrt(999)), x^x) &latex",
        "cpi(floor(sqrt(6)), ceiling(cbrt(999)), x^x) &latexdoc",
        "cpi(floor(sqrt(6)), ceiling(cbrt(999)), x^x) &render",
        "sigma(1, 100, x^2-2*x+1) &step",
        "1+(2+a) &a=10 &step &render"
    };
}