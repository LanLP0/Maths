using System.Threading.Tasks;
using DiffEngine;
using VerifyTests;
using VerifyXunit;
using Xunit;
using static VerifyXunit.Verifier;

// ReSharper disable InconsistentNaming

namespace LCalc.Tests;

/// <remarks>Note: All tests should be run in DEBUG</remarks>
[UsesVerify]
public sealed class CalculatorTest
{
    private static readonly VerifySettings _settings;

    static CalculatorTest()
    {
        _settings = new VerifySettings();
        _settings.UseDirectory("test-results");
        DiffRunner.MaxInstancesToLaunch(1);
    }

    [Fact]
    public Task Result_Should_BeExpected()
    {
        // Arrange
        var cases = TestCases.TestCases.Result_Should_BeExpected;
        var results = new CalcTestResult[cases.Length];

        // Act
        for (var i = 0; i < cases.Length; i++)
        {
            var c = cases[i];

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

    [Fact]
    public Task Error_ShouldBe_Expected()
    {
        // Arrange
        var cases = TestCases.TestCases.Error_ShouldBe_Expected;
        var results = new CalcTestResult[cases.Length];

        // Act
        for (var i = 0; i < cases.Length; i++)
        {
            var c = cases[i];

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

    [Fact]
    public Task Render_ShouldBe_Expected()
    {
        // Arrange
        var cases = TestCases.TestCases.Render_ShouldBe_Expected;
        var results = new CalcTestResult[cases.Length];

        // Act
        for (var i = 0; i < cases.Length; i++)
        {
            var c = cases[i];

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