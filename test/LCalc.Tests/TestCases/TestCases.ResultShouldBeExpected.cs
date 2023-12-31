// ReSharper disable InconsistentNaming

namespace LCalc.Tests.TestCases;

public static partial class TestCases
{
    public static readonly string[] Result_Should_BeExpected =
    {
        // Misc
        "-1",
        "2*-1",
        "1+2",
        "1-2",
        "1*2",
        "25%6",
        "25%*8",
        "2^3^2",
        "3!",
        "0!",
        "1/2",
        "1/4",
        "1/8",
        "1/3",
        "((1))",
        "0123",
        "-1",
        "a.5 &a=4",
        ".5",
        "1-|2-(3+(4", // Implicit braces
        // Variable
        "3a&a=2",
        "a&a=1",
        "1-&a=70a",
        "a3&a=2",
        "a3&a=-2",
        // Computed variable
        "a &b=2 &a=(b^3)",
        "a&a=(abs(-15))",
        "a&a=(1+(2^(3-4",
        // Special value
        "25%",
        "-0x1e240",
        "0x1e240",
        "0o361100",
        "0b11110001001000000",
        // Custom function
        "[f()=0x1e240]f()",
        "[f()=0o361100]f()",
        "[f()=0b11110001001000000]f()",
        // Bitwise
        "2&3",
        "~2",
        "2||3",
        "1||6",
        "2<<3",
        "5>>2",
        "12^^10",
        // Compare
        "1!=0<=2>=2==2<3>1",
        "1>2",
        // Function
        "sum(1 2 3)",
        "avg(1 2 3 4 5)",
        "abs(-1)",
        "|-abs(-|-1|)|",
        "1+|-2|",
        "cbrt(8)",
        "sqrt(4)",
        "ceiling(4.5)",
        "round(5.5)",
        "round(1.123456789 5) &raw",
        "floor(4.5)",
        "gcd(6 9)",
        "lcm(1 2 3 4)",
        "gcd(0 3)",
        "gcd(12 0)",
        "sin(1)",
        "cos(1)",
        "tan(1)",
        "cot(38)",
        "log(3)",
        // sigma() & cpi()
        "sigma(1 4 x*10)", // Implicit variable
        "cpi(1 4 x*10)",
        "sigma(x 1 4 x*10)",
        "cpi(x 1 4 x*10)",
        // Custom function
        "[foo(a b c)=a^(b+c)]foo(2 1 foo(2 1 1))",
        "[foo(a)=a%6] foo(25)",
        "[foo(a)=a%] foo(25)",
        "[foo(a)=a%*8] foo(25)",
        "[t()=a()][a()=c] t() &c=1",
        "[f(a)=a] f(1) &a=2",
        "[f(a) a] f(1) &a=2",
        "[a()=1] [b(x)=x] a()",
        // Format
        "134/3 &fmt=human",
        "134/3 &fmt=raw",
        "134/3 &fmt=hex",
        "134/3 &fmt=oct",
        "134/3 &fmt=bin",
        "1/0 &fmt=hex",
        "1/0 &fmt=oct",
        "1/0 &fmt=bin",
        "-134/3 &fmt=raw",
        "-134/3 &fmt=hex",
        "-134/3 &fmt=oct",
        "-134/3 &fmt=bin",
        "-1/0 &fmt=hex",
        "-1/0 &fmt=oct",
        "-1/0 &fmt=bin",
        // Solve
        "x^5-2x+1 &solve",
        "x+1 &solve",
        "[f(x)=x^2-2x+1] f(x) &solve",
        "x!-6 &solve",
        "sqrt((x+1))+1==4x^2+sqrt((3x)) &solve"
    };
}