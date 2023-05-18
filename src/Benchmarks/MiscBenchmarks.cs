using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Common.Results;
using LCalc;
using LCalc.MathTree;
using LCalc.MathTree.Nodes;
using static Common.Results.ResultHelpers;

namespace Benchmarks;

// [MemoryDiagnoser]
[MaxIterationCount(50)]
public class MiscBenchmarks
{
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

    private IMathNode _node;
    private Scope _scope;

    [GlobalSetup]
    public void Setup()
    {
        _scope = new Scope(false);
        _node = new FunctionCallNode("abs");
        _node.AddNode(new ValueNode(-69));
    }

    [Benchmark]
    public Result<double> Calc()
    {
        return _node.Calc(_scope);
    }
}