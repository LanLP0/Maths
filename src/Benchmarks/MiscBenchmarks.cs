using BenchmarkDotNet.Attributes;
using Common.Results;
using LCalc;
using LCalc.MathTree;
using LCalc.MathTree.Nodes;

namespace Benchmarks;

[MemoryDiagnoser]
[MaxIterationCount(50)]
public class MiscBenchmarks
{
    [Benchmark]
    public Result ResultOk()
    {
        return ResultHelpers.Ok();
    }
    
    // [Params(1.123, 1230.1, 1020304050.321)]
    // public double Num1 { get; set; }
    //
    // [Params(1.123, 1230.1, 1020304050.321)]
    // public double Num2 { get; set; }
    //
    // [Benchmark(Baseline = true)]
    // public double Add()
    // {
    //     return Num1 + Num2;
    // }
    //
    // [Benchmark]
    // public double Subtract()
    // {
    //     return Num1 - Num2;
    // }
    //
    // [Benchmark]
    // public double Multiply()
    // {
    //     return Num1 * Num2;
    // }
    //
    // [Benchmark]
    // public double Divide()
    // {
    //     return Num1 / Num2;
    // }
    //
    // [Benchmark]
    // public double Modulo()
    // {
    //     return Num1 % Num2;
    // }

    // private IMathNode _node;
    // private Scope _scope;
    //
    // [GlobalSetup]
    // public void Setup()
    // {
    //     _scope = new Scope(false);
    //     _node = new ValueNode(1);
    // }
    //
    // [Benchmark]
    // public Result<double> Calc()
    // {
    //     return _node.Calc(_scope);
    // }
}