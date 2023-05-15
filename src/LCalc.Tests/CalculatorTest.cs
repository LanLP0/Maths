using Xunit;

namespace LCalc.Tests;

public sealed class CalculatorTest
{
    [Theory]
    // Normal
    [InlineData("-1", "-1")]
    [InlineData("2*-1", "-2")]
    [InlineData("1+2", "3")]
    [InlineData("1-2", "-1")]
    [InlineData("1*2", "2")]
    [InlineData("25%6", "1")]
    [InlineData("25%*8", "2")]
    [InlineData("2^3^2", "512")]
    [InlineData("3!", "6")]
    [InlineData("0!", "1")] // Special case: 0! = 1
    // Rounding
    [InlineData("1/2", "0.5")]
    [InlineData("1/4", "0.25")]
    [InlineData("1/8", "1/8")]
    [InlineData("1/3", "1/3")]
    [InlineData("((1))", "1")]
    // Variable
    [InlineData("3a&a=2", "6")]
    [InlineData("a&a=1", "1")]
    [InlineData("1-&a=70a", "-69")]
    [InlineData("a3&a=2", "8")]
    [InlineData("a3&a=-2", "-8")]
    // Special number
    [InlineData("25%", "0.25")]
    [InlineData("0x1e240", "123456")]
    [InlineData("0o361100", "123456")]
    [InlineData("0b11110001001000000", "123456")]
    // Special number in a function
    [InlineData("[f()=0x1e240]f()", "123456")]
    [InlineData("[f()=0o361100]f()", "123456")]
    [InlineData("[f()=0b11110001001000000]f()", "123456")]
    // Bitwise
    [InlineData("2&3", "2")]
    [InlineData("~2", "-3")]
    [InlineData("2|3", "3")]
    [InlineData("1|6", "7")]
    [InlineData("2<<3", "16")]
    [InlineData("5>>2", "1")]
    [InlineData("12^^10", "6")]
    // Compare
    [InlineData("1!=0<=2>=2==2<3>1", "True")]
    [InlineData("1>2", "False")]
    // Function
    [InlineData("sum(1 2 3)", "6")]
    [InlineData("avg(1 2 3 4 5)", "3")]
    [InlineData("abs(-1)", "1")]
    [InlineData("cbrt(8)", "2")]
    [InlineData("sqrt(4)", "2")]
    [InlineData("ceiling(4.5)", "5")]
    [InlineData("round(5.5)", "6")]
    [InlineData("round(1.123456789 5) &raw", "1.12346")]
    [InlineData("floor(4.5)", "4")]
    [InlineData("gcd(6 9)", "3")]
    [InlineData("lcm(1 2 3 4)", "12")]
    [InlineData("gcd(0 3)", "3")]
    [InlineData("gcd(12 0)", "0")]
    [InlineData("sin(1)", "0.017452")]
    [InlineData("cos(1)", "0.999848")]
    [InlineData("tan(1)", "0.017455")]
    [InlineData("cot(38)", "1.279942")]
    [InlineData("log(3)", "1.098612")]
    [InlineData("sigma(x 1 4 x*10)", "100")]
    [InlineData("cpi(x 1 4 x*10)", "240000")]
    // Custom function
    [InlineData("[foo(a b c)=a^(b+c)]foo(2 1 foo(2 1 1))", "32")]
    [InlineData("[foo(a)=a%6] foo(25)", "1")]
    [InlineData("[foo(a)=a%] foo(25)", "0.25")]
    [InlineData("[foo(a)=a%*8] foo(25)", "2")]
    [InlineData("[t()=a()][a()=c] t() &c=1", "1")]
    [InlineData("[f(a)=a] f(1) &a=2", "1")]
    [InlineData("[f(a) a] f(1) &a=2", "1")]
    [InlineData("[a()=1] [b(x)=x] a()", "1")] // Variables is not shared between functions
    // Solve mode
    [InlineData("x^5-2x+1 &solve", "0.51879")]
    [InlineData("x+1 &solve", "-1")]
    [InlineData("[f(x)=x^2-2x+1] f(x) &solve", "0.999991")]
    [InlineData("x!-6 &solve", "3")]
    public void Result_Should_BeExpected(string math, string result)
    {
        // Arrange
        var output = $"Result: {result}";

        // Act
        var result1 = Calculator.CalcFormatted(math);

        // Assert
        Assert.Equal(output, result1);
    }

    [Theory]
    [InlineData("a,b", "',' can only be used in function calls")]
    [InlineData("a+(b,c)", "',' can only be used in function calls")]
    [InlineData("null()", "Unknown function null()")]
    [InlineData("sum()", "sum() takes at least one argument")]
    [InlineData("avg()", "avg() takes at least one argument")]
    [InlineData("abs()", "abs() accept exactly 1 argument")]
    [InlineData("cbrt()", "cbrt() accept exactly 1 argument")]
    [InlineData("sqrt()", "sqrt() accept exactly 1 argument")]
    [InlineData("ceiling()", "ceiling() accept exactly 1 argument")]
    [InlineData("round()", "round() accept exactly 1 - 2 argument")]
    [InlineData("floor()", "floor() accept exactly 1 argument")]
    [InlineData("gcd()", "gcd() require at least 1 argument")]
    [InlineData("lcm()", "lcm() require at least 1 argument")]
    [InlineData("random(1 2 3)", "random() takes at most 2 arguments")]
    [InlineData("cos()", "cos() takes exactly one argument")]
    [InlineData("sin()", "sin() takes exactly one argument")]
    [InlineData("tan()", "tan() takes exactly one argument")]
    [InlineData("cot()", "cot() takes exactly one argument")]
    [InlineData("log()", "log() takes exactly one argument")]
    [InlineData("random(1 a)", "Unknown variable 'a'")]
    [InlineData("log()&step", "log() takes exactly one argument")]
    [InlineData("", "No expression found")]
    [InlineData("(1", "Invalid number of braces")]
    [InlineData("1==(1", "Invalid number of braces")]
    [InlineData("((1)&step", "Invalid number of braces")]
    [InlineData("1-", "Missing value after operator -")]
    [InlineData("null!", "Unknown variable 'null'")]
    [InlineData("2.5<<2.5", "Value(s) of operator << must be integer")]
    [InlineData("50!<<50!", "Value(s) of operator << must be between 2^31 and -2^31")]
    [InlineData("2.5|2.5", "Value(s) of operator | must be integer")]
    [InlineData("100!|100!", "Value(s) of operator | must be between 2^63 and -2^63")]
    [InlineData("-n", "Unknown variable 'n'")]
    [InlineData("1~", "Invalid operator ~")]
    [InlineData("1&", "Missing value after operator &")]
    [InlineData("1+", "Missing value after operator +")]
    [InlineData("+1", "Missing value before operator +")]
    [InlineData("1 1", "Missing operator")]
    [InlineData("1-&a=100", "Missing value after operator -")]
    [InlineData("&a=", "Missing variable value")]
    [InlineData("&a=1&a=1", "Variable 'a' had already been set")]
    [InlineData("0b21", "Invalid binary number")]
    [InlineData("0o9", "Invalid octal number")]
    [InlineData("[t()=t()]t()", "Function loop is not allowed")]
    [InlineData("[a()=)]", "Invalid amount of braces in custom function")]
    [InlineData("[a(x x)=1]", "Duplicated variables in custom function")]
    [InlineData("[a(])", "Invalid custom function syntax")]
    [InlineData("[a(1)]", "Invalid character in custom function arg space '1'")]
    [InlineData("[a()=1", "Invalid custom function syntax")]
    [InlineData("[a():1]", "Invalid custom function syntax")]
    [InlineData("[a()1]", "Invalid custom function syntax")]
    // Sigma & CPi
    [InlineData("sigma(x, 3, 1, x*10)", "sigma(): End cannot be less than start")]
    [InlineData("sigma(x, -3, 3)", "sigma() takes exactly 4 arguments")]
    [InlineData("sigma(y, -3, 3, x*10)", "sigma(): Invalid variable name")]
    [InlineData("sigma(x, -3, 3, 10)", "sigma(): Invalid variable name")]
    [InlineData("sigma(x-1, 1, 1, x)", "sigma(): First argument must be a variable")]
    [InlineData("sigma(x, 1.1, 2, x)", "sigma(): Start must be an integer")]
    [InlineData("sigma(x, 1, 2.1, x)", "sigma(): End must be an integer")]
    [InlineData("cpi(x, 3, 1, x*10)", "cpi(): End cannot be less than start")]
    [InlineData("cpi(x, -3, 3)", "cpi() takes exactly 4 arguments")]
    [InlineData("cpi(y, -3, 3, x*10)", "cpi(): Invalid variable name")]
    [InlineData("cpi(x, -3, 3, 10)", "cpi(): Invalid variable name")]
    [InlineData("cpi(x-1, 1, 1, x)", "cpi(): First argument must be a variable")]
    [InlineData("cpi(x, 1.1, 2, x)", "cpi(): Start must be an integer")]
    [InlineData("cpi(x, 1, 2.1, x)", "cpi(): End must be an integer")]
    // Solve mode
    [InlineData("abs(x)+1 &solve", "Cannot solve"
#if DEBUG
                                   + " (15 tries ended)"
#endif
    )]
    [InlineData("1 &solve", "No unknown to solve for")]
    [InlineData("x*y &solve", "Too many unknowns")]
    public void Calc_Should_Error(string math, string errorMsg)
    {
        // Arrange
        var output = $"Error: {errorMsg}";

        // Act
        var result = Calculator.CalcFormatted(math);

        // Assert
        Assert.Equal(output, result);
    }

    [Theory]
    [InlineData("1+(1+1)&step",
        """
        1 + (1 + 1)
        1 + 2
        Result: 3
        """)]
    [InlineData("[foo(a b c)=a^(b+c)]foo(2 1 (1+foo(2 1 1))) &step",
        """
        foo(2, 1, (1 + foo(2, 1, 1)))
        foo(2, 1, (1 + 4))
        foo(2, 1, 5)
        Result: 64
        """)]
    [InlineData("abs(sin(((~1>>2<<2)^2!/1000*50-40+1))) &step",
        """
        abs(sin((((~1 >> 2 << 2) ^ 2!) / 1000 * 50 - 40 + 1)))
        abs(sin(((-4 ^ 2!) / 1000 * 50 - 40 + 1)))
        abs(sin((20922789888000 / 1000 * 50 - 40 + 1)))
        abs(sin(1046139494361))
        abs(-0.6293194965251864)
        Result: 0.629319
        """)]
    [InlineData("abs(sin(((~1>>2<<2)^2!/1000*50-40+1))) &tree",
        """
        abs(sin((((((((((~1) >> 2) << 2) ^ 2)!) / 1000) * 50) - 40) + 1)))
        abs(sin(((((((-4 ^ 2)!) / 1000) * 50) - 40) + 1)))
        abs(sin(((((20922789888000 / 1000) * 50) - 40) + 1)))
        abs(sin(1046139494361))
        abs(-0.6293194965251864)
        Result: 0.629319
        """)]
    [InlineData("abs(1+(2+3))==2+4!= (1+(2+(3+4))) &step",
        """
        abs(1 + (2 + 3)) == 2 + 4 != (1 + (2 + (3 + 4)))
        abs(1 + (2 + 3)) == 2 + 4 != (1 + (2 + 7))
        abs(1 + 5) == 2 + 4 != (1 + 9)
        6 == 2 + 4 != 10
        6 == 6 != 10
        Result: True
        """)]
    public void Calc_Should_ReturnCorrectStep(string math, string output)
    {
        // Act
        var result = Calculator.CalcFormatted(math);

        // Assert
        Assert.Equal(output, result);
    }

    [Theory]
    [InlineData("random()", 0, 1)]
    [InlineData("random(5)", 0, 5)]
    [InlineData("random(5 10)", 5, 10)]
    public void Random_Should_BeInRange(string math, double lowerEnd, double upperEnd)
    {
        for (var i = 0; i <= 100; i++)
        {
            // Act
            var result = Calculator.CalcRaw(math, out _);
            var num = result.Number!;

            // Assert
            Assert.InRange(num.Value, lowerEnd, upperEnd);
        }
    }
}