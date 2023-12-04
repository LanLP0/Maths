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
    // [Params("1",
    //     "1233211 + 227633 * 6555 + 999 / (99+1)",
    //     "sin(cos(tan(9)))",
    //     "abs(sin(((~1>>2<<2)^2!/1000*50-40+1)))",
    //     "[a()=1]a()",
    //     "[a()=1][b()=1][c()=1][d()=1][e()=1][f()=1][g()=1][h()=1][j()=1][q()=1][k()=1]a()",
    //     "sigma(x, 1, 1000000, x)"
    // )]
    // // ReSharper disable once MemberCanBePrivate.Global
    // public string Math { get; set; } = null!;
    //
    // [Benchmark(Baseline = true)]
    // public void MathTreeParse()
    // {
    //     var tree = new MathTree();
    //     tree.Parse(Math);
    // }
    //
    // [Benchmark]
    // public void NewParser()
    // {
    //     var parser = new ExpParser();
    //     parser.Parse(Math);
    // }

    // [Benchmark]
    // public Result ResultOk()
    // {
    //     return ResultHelpers.Ok();
    // }

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