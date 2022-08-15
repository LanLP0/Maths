using BenchmarkDotNet.Attributes;
using LCalc;
using LCalc.CustomFunction;
using LCalc.Helpers;

namespace Benchmarks;

[MemoryDiagnoser]
public class LCalcBenchmarks
{
    [Params("1", "1233211 + 227633 * 6555 + 999 / (99+1)", "2^2^2^2", "sin(cos(tan(9)))")]
    public string Math { get; set; } = null!;

    private (List<CalcElement>, CustomFunctionCollection) val;

    [GlobalSetup]
    public void Setup()
    {
        var splitResult =
            CalculatorHelpers.SplitInput(Math, out var args, out var opts, out var functions);

        val = (splitResult.Value!, functions);
    }
    
    [Benchmark]
    public string LCalc()
    {
        return Calculator.Calc(Math);
    }

    [Benchmark(Baseline = true)]
    public double LCalcRaw()
    {
        return Calculator.Calculate(val.Item1, val.Item2).Value;
    }
}