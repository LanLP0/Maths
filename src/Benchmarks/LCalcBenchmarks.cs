using BenchmarkDotNet.Attributes;
using LCalc;

namespace Benchmarks;

[MemoryDiagnoser]
public class LCalcBenchmarks
{
    [Params("1", "1233211 + 227633 * 6555 + 999 / (99+1)", "2^2^2^2", "sin(cos(tan(9)))")]
    public string Math { get; set; } = null!;

    [Benchmark(Baseline = true)]
    public void LCalc()
    {
        Calculator.Calc(Math);
    }
}