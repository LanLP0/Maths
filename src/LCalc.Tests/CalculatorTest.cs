using Xunit;

namespace LCalc.Tests;

internal class CalculatorTest
{
    [Theory]
    // Normal
    [InlineData("-1", "-1")]
    [InlineData("2*-1", "-2")]
    [InlineData("1+2", "3")]
    [InlineData("1-2", "-1")]
    [InlineData("1*2", "2")]
    [InlineData("1/2", "1/2")]
    [InlineData("7%4", "3")]
    [InlineData("((1))", "1")]
    [InlineData("3a&a=2", "6")]
    [InlineData("a&a=1", "1")]
    [InlineData("1-&a=70a", "-69")]
    [InlineData("a3&a=2", "8")]
    [InlineData("2^3^2", "512")]
    [InlineData("3!", "6")]
    [InlineData("&h1e240", "123456")]
    [InlineData("&o361100", "123456")]
    [InlineData("&b11110001001000000", "123456")]
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
    [InlineData("sin(1)", "0.01745240643728351")]
    [InlineData("cos(1)", "0.9998476951563913")]
    [InlineData("tan(1)", "0.017455064928217585")]
    [InlineData("cot(38)", "1.2799416321930788")]
    [InlineData("log(3)", "1.0986122886681098")]
    // Custom function
    [InlineData("[null(a b c)=a^(b+c)]null(2 1 null(2 1 1))", "32")]
    public void Result_Should_BeExpected(string math, string result)
    {
        // Arrange
        var output = $"Result: {result}";
        
        // Act
        var result1 = Calculator.Calc(math);
        
        // Assert
        Assert.Equal(output, result1);
    }

    [Theory]
    [InlineData("null()", "Unknown function: null()")]
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
    [InlineData("random(1 a)", "a is not a number")]
    [InlineData("log()&step", "log() takes exactly one argument")]
    [InlineData("", "No expression found")]
    [InlineData("(1", "Invalid number of brackets")]
    [InlineData("1==(1", "Invalid number of brackets")]
    [InlineData("((1)&step", "Invalid number of brackets")]
    [InlineData("1-", "No value after -")]
    [InlineData("null!", "null is not an integer")]
    [InlineData("2.5<<2.5", "2.5 is not an integer")]
    [InlineData("50!<<50!", "Value too big")]
    [InlineData("2.5|2.5", "2.5 is not an integer")]
    [InlineData("100!|100!", "Value too big")]
    [InlineData("-n", "-n is not a number")]
    [InlineData("1~", "No value after ~")]
    [InlineData("1&", "No value after &")]
    [InlineData("1+", "No value after +")]
    [InlineData("+1", "No value before +")]
    [InlineData("1 1", "Missing operator")]
    [InlineData("1-&a=100", "Missing value")]
    [InlineData("&a=", "Missing variable value")]
    [InlineData("&a=1&a=1", "Variable has already been set")]
    [InlineData("&b21", "Invalid binary number")]
    [InlineData("&o9", "Invalid octal number")]
    [InlineData("[t()=t()]t()", "Cannot call a function in it-self")]
    public void Calc_Should_Error(string math, string errorMsg)
    {
        // Arrange
        var output = $"Error: {errorMsg}";
        
        // Act
        var result = Calculator.Calc(math);
        
        // Assert
        Assert.Equal(output, result);
    }

    [Theory]
    [InlineData("1+(1+1)&step", "1 + 2\n\nResult: 3")]
    [InlineData("((1))&step", "( 1 )\n1\n\nResult: 1")]
    [InlineData("(sum(1))&step", "( 1 )\n1\n\nResult: 1")]
    [InlineData("1*2&step", "\nResult: 2")]
    public void Calc_Should_ReturnCorrectStep(string math, string output)
    {
        // Act
        var result = Calculator.Calc(math);
        
        // Assert
        Assert.Equal(output, result);
    }

    [Theory]
    [InlineData("random()", 0, 1)]
    [InlineData("random(5)", 0, 5)]
    [InlineData("random(5 10)", 5, 10)]
    public void Random_Should_BeInRange(string math, double lowerEnd, double upperEnd)
    {
        for (int i = 0; i <= 25; i++) // Run the test 25 times
        {
            // Act
            var result = Calculator.Calc(math);
            var resultNum = double.Parse(result.Substring(result.IndexOf(' ') + 1));
        
            // Assert
            Assert.InRange(resultNum, lowerEnd, upperEnd);
        }
    }
}