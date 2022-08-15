using System.Text;
using BenchmarkDotNet.Attributes;
using LToolBox;
using LToolBox.Tools;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    // private Tool[] _tools = null!;
    //
    // private string _promptTemplate = null!;
    //
    // [Benchmark]
    // public void LToolBoxInit()
    // {
    //     Console.CancelKeyPress += (_, _) =>
    //     {
    //         Console.ForegroundColor = ConsoleColor.White;
    //         Environment.Exit(0);
    //     };
    //
    //     var serviceProvider = new ServiceCollection()
    //         .AddLogging(a =>
    //         {
    //             var logger = new LoggerConfiguration()
    //                 .MinimumLevel.Verbose()
    //                 .WriteTo.Console();
    //
    //             a.AddSerilog(logger.CreateLogger());
    //         })
    //         .AddSingleton<Tool, ComplexMultiplyTool>()
    //         .AddSingleton<Tool, DeltaTool>()
    //         .AddSingleton<Tool, LCalcTool>()
    //         .AddSingleton<Tool, PolynomialTool>()
    //         .AddSingleton<Tool, FactTool>()
    //         .AddSingleton<Tool, IsPrimeTool>()
    //         .BuildServiceProvider();
    //
    //     _tools = serviceProvider.GetServices<Tool>().ToArray();
    //
    //     StringBuilder buffer = new();
    //     buffer.Append("> ");
    //     var count = 1;
    //     foreach (var tool in _tools)
    //     {
    //         buffer.Append(count);
    //         buffer.Append('-');
    //         buffer.Append(tool.ToolName);
    //         buffer.Append(' ');
    //
    //         count++;
    //     }
    // }

    [Params('a', 'b', '\0')]
    public char Chr = default;

    [Benchmark(Baseline = true)]
    public bool IsChr()
    {
        return Chr is 'a';
    }

    [Benchmark]
    public bool EqChr()
    {
        return Chr == 'a';
    }
    
    [Benchmark]
    public bool IsChrInt()
    {
        return (int)Chr is 97;
    }

    [Benchmark]
    public bool EqChrInt()
    {
        return (int)Chr == 97;
    }
}