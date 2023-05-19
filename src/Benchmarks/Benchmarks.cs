using BenchmarkDotNet.Attributes;
using Common.Results;
using LCalc;
using LCalc.MathTree;

namespace Benchmarks;

[MemoryDiagnoser]
[MaxIterationCount(50)]
public class Benchmarks
{
    [Params("1",
        "1233211 + 227633 * 6555 + 999 / (99+1)", "sin(cos(tan(9)))",
        "abs(sin(((~1>>2<<2)^2!/1000*50-40+1)))",
        "[a()=1]a()",
        "[a()=1][b()=1][c()=1][d()=1][e()=1][f()=1][g()=1][h()=1][j()=1][q()=1][k()=1]a()",
        "sigma(x, 1, 1000000, x)"
        )]
    // ReSharper disable once MemberCanBePrivate.Global
    public string Math { get; set; } = null!;

    private MathTree Tree { get; set; } = null!;

    // [Benchmark]
    // public void LCalc()
    // {
    //     Calculator.CalcFormatted(Math);
    // }

    // [Benchmark(Baseline = true)]
    [Benchmark(Baseline = true)]
    public void LCalcRaw()
    {
        Calculator.CalcRaw(Math, out _);
    }

    [Benchmark]
    public void LCalcParse()
    {
        var tree = new MathTree();
        tree.Parse(Math);
    }

    [Benchmark]
    public Result<double> LCalcCalc()
    {
        return Tree.GetTopNode().Calc(Tree.Scope);
    }

    [GlobalSetup]
    public void Setup()
    {
        Tree = new MathTree();
        Tree.Parse(Math);
    }

    // [GlobalSetup]
    // public void Setup()
    // {
    //     var args = new[]{"abc", "def", "ghk", "lalala", "jajaja"};
    //
    //     for (var i = 0; i < args.Length; i++)
    //     {
    //         var arg = args[i];
    //         var id = StringToId(arg);
    //         args1.Add(id, i);
    //         args2.Add(arg, i);
    //         args3.Add(new Test(arg, id));
    //     }
    // }
    //
    // private Dictionary<int, double> args1 = new();
    // private Dictionary<string, double> args2 = new();
    // private List<Test> args3 = new();
    //
    // [Params("a", "abcdefghkj", "abc")]
    // public string arg;

    // private static long StringToId(ReadOnlySpan<char> s)
    // {
    //     var length = s.Length;
    //     var result = 0L;
    //     const long radix = 26;
    //
    //     for (var i = 0; i < length; i++)
    //     {
    //         var chr = s[i] - 65;
    //
    //         result = result * radix + chr;
    //     }
    //
    //     return result;
    // }

    // [Benchmark]
    // public void Id()
    // {
    //     args1.TryGetValue(StringToId(arg), out _);
    // }
    //
    // [Benchmark]
    // public void String()
    // {
    //     args2.TryGetValue(arg, out _);
    // }
    //
    // [Benchmark]
    // public void FindId()
    // {
    //     args3.FindIndex(a => a.Name == arg);
    // }
    //
    // [Benchmark]
    // public void FindString()
    // {
    //     args3.FindIndex(a => a.Id == StringToId(arg));
    // }

    // public record Test(string Name, long Id);
    //
    // public List<Test> List1 = new();
    // public List<long> List2 = new();
    // public Dictionary<long, Test> Dict = new();
    // [Params("ghij", "ab", "asndjsahidsag")]
    // public string ChosenId;
    //
    // [GlobalSetup]
    // public void Setup()
    // {
    //     List1.Clear();
    //     List2.Clear();
    //     Dict.Clear();
    //
    //     var names = new[] {"a", "bc", "def", "ghij", "klmno", "pqrstu", "vwxyz"};
    //     
    //     foreach (var name in names)
    //     {
    //         var id = StringToId(name);
    //         var item = new Test(name, id);
    //         List1.Add(item);
    //         List2.Add(id);
    //         Dict.Add(id, item);
    //     }
    // }
    //
    // [Benchmark]
    // public Test? ListCustom()
    // {
    //     var id = StringToId(ChosenId);
    //     foreach (var item in List1)
    //     {
    //         if (item.Id == id)
    //             return item;
    //     }
    //     return null;
    // }
    //
    // [Benchmark]
    // public Test? List()
    // {
    //     var index = List1.FindIndex(a => a.Id == StringToId(ChosenId));
    //     if (index is -1)
    //         return null;
    //     return List1[index];
    // }
    //
    // [Benchmark]
    // public Test? DualList()
    // {
    //     var index = List2.IndexOf(StringToId(ChosenId));
    //     if (index is -1)
    //         return null;
    //     return List1[index];
    // }
    //
    // [Benchmark]
    // public Test? Dictionary()
    // {
    //     if (!Dict.TryGetValue(StringToId(ChosenId), out var result))
    //         return null;
    //     return result;
    // }

    // [Params(0, 10, 100)]
    // public int Amount;
    //
    // [Benchmark]
    // public List<Test> List()
    // {
    //     var list = new List<Test>();
    //
    //     for (var i = 0; i < Amount; i++)
    //     {
    //         list.Add(new Test(i.ToString(), i));
    //     }
    //
    //     return list;
    // }
    //
    // [Benchmark]
    // public List<Test> DualList()
    // {
    //     var list = new List<Test>();
    //     var list1 = new List<int>();
    //
    //     for (var i = 0; i < Amount; i++)
    //     {
    //         list.Add(new Test(string.Empty, i));
    //         list1.Add(i);
    //     }
    //
    //     return list;
    // }
    //
    // [Benchmark]
    // public Dictionary<int, Test> Dictionary()
    // {
    //     var dict = new Dictionary<int, Test>();
    //     
    //     for (var i = 0; i < Amount; i++)
    //     {
    //         dict.Add(i, new Test(string.Empty, i));
    //     }
    //
    //     return dict;
    // }
}