using System.Threading.Tasks;
using DiffEngine;
using VerifyTests;
using VerifyXunit;
using Xunit;
using static VerifyXunit.Verifier;
// ReSharper disable InconsistentNaming

namespace LCalc.Tests;

[UsesVerify]
public sealed class CalculatorTest
{
    private static readonly VerifySettings _settings;

    static CalculatorTest()
    {
        _settings = new VerifySettings();
        _settings.UseDirectory("test-results");
        DiffRunner.MaxInstancesToLaunch(3);
    }

    private readonly string[] Result_Should_BeExpected_Cases = {
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
        "x!-6 &solve"
    };
    
    [Fact]
    public Task Result_Should_BeExpected()
    {
        // Arrange
        var results = new CalcTestResult[Result_Should_BeExpected_Cases.Length];
        
        // Act
        for (var i = 0; i < Result_Should_BeExpected_Cases.Length; i++)
        {
            var c = Result_Should_BeExpected_Cases[i];
            
            var result = Calculator.CalcFormatted(c);
            results[i] = new CalcTestResult
            {
                Math = c,
                Output = result
            };
        }

        // Assert
        return Verify(results, _settings);
    }

    private readonly string[] Error_ShouldBe_Expected_Cases = {
        // Function
        "1,2",
        "1+(2,3)",
        "null()",
        "sum()",
        "avg()",
        "abs()",
        "cbrt()",
        "sqrt()",
        "ceiling()",
        "round()",
        "round(1.234, 6)",
        "floor()",
        "gcd()",
        "gcd(1.1 2.2)",
        "gcd(1 2.2)",
        "lcm()",
        "lcm(1.1 2.2)",
        "lcm(1 2.2)",
        "random(1 2 3)",
        "random(1 a)",
        "cos()",
        "sin()",
        "tan()",
        "cot()",
        "log()",
        "|-1)",
        "sin(,,,)",
        // Misc
        "",
        "()",
        "1)",
        "1-",
        "null!",
        "-2!",
        "2.5<<2.5",
        "50!<<50!",
        "2.5||2.5",
        "100!||100!",
        "-n",
        "1~",
        "1&",
        "1+",
        "+1",
        "1 1",
        "2^3^ ^",
        "1-&a=100",
        "&a=",
        "&a=1&a=1",
        "1 &fmt=error",
        // Special number
        "0x",
        "0b",
        "0o",
        "0xfg",
        "0b12",
        "0o89",
        // Custom function
        "[t()=t()]t()",
        "[a()=)]",
        "[a(x x)=1]",
        "[a(])",
        "[a(1)]",
        "[a()=1",
        "[a():1]",
        "[a()1]",
        "[a()=1] a(2)",
        // Sigma & CPi
        "sigma(x, 3, 1, x*10)",
        "sigma(x, -3, 3)",
        "sigma(y, -3, 3, x*10)",
        "sigma(x, -3, 3, 10)",
        "sigma(x-1, 1, 1, x)",
        "sigma(x, 1.1, 2, x)",
        "sigma(x, 1, 2.1, x)",
        "cpi(x, 3, 1, x*10)",
        "cpi(x, -3, 3)",
        "cpi(y, -3, 3, x*10)",
        "cpi(x, -3, 3, 10)",
        "cpi(x-1, 1, 1, x)",
        "cpi(x, 1.1, 2, x)",
        "cpi(x, 1, 2.1, x)",
        // Solve
        "abs(x)+1 &solve",
        "1 &solve",
        "x*y &solve"
    };

    [Fact]
    public Task Error_ShouldBe_Expected()
    {
        // Arrange
        var results = new CalcTestResult[Error_ShouldBe_Expected_Cases.Length];
        
        // Act
        for (var i = 0; i < Error_ShouldBe_Expected_Cases.Length; i++)
        {
            var c = Error_ShouldBe_Expected_Cases[i];
            
            var result = Calculator.CalcFormatted(c);
            results[i] = new CalcTestResult
            {
                Math = c,
                Output = result
            };
        }

        // Assert
        return Verify(results, _settings);
    }

    private readonly string[] Step_ShouldBe_Expected_Cases = {
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

    [Fact]
    public Task Step_ShouldBe_Expected()
    {
        // Arrange
        var results = new CalcTestResult[Step_ShouldBe_Expected_Cases.Length];
        
        // Act
        for (var i = 0; i < Step_ShouldBe_Expected_Cases.Length; i++)
        {
            var c = Step_ShouldBe_Expected_Cases[i];
            
            var result = Calculator.CalcFormatted(c);
            results[i] = new CalcTestResult
            {
                Math = c,
                Output = result
            };
        }

        // Assert
        return Verify(results, _settings);
    }

    [Theory]
    [InlineData("random()", 0, 1)]
    [InlineData("random(5)", 0, 5)]
    [InlineData("random(5 10)", 5, 10)]
    public void Random_ShouldBe_InExpectedRange(string math, double lowerEnd, double upperEnd)
    {
        for (var i = 0; i <= 100; i++)
        {
            // Act
            var result = Calculator.CalcRaw(math);
            var num = result.Number!;

            // Assert
            Assert.InRange(num.Value, lowerEnd, upperEnd);
        }
    }
}